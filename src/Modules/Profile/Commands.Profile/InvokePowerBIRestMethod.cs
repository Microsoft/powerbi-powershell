/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerBI.Commands.Common;
using Microsoft.PowerBI.Common.Abstractions.Interfaces;
using Microsoft.PowerBI.Common.Abstractions.Utilities;


namespace Microsoft.PowerBI.Commands.Profile
{
    [Cmdlet(CmdletVerb, CmdletName)]
    [OutputType(typeof(string))]
    public class InvokePowerBIRestMethod : PowerBICmdlet
    {
        // Similiar to Invoke-RestMethod - https://github.com/PowerShell/PowerShell/blob/master/src/Microsoft.PowerShell.Commands.Utility/commands/utility/WebCmdlet/Common/InvokeRestMethodCommand.Common.cs

        public const string CmdletVerb = VerbsLifecycle.Invoke;
        public const string CmdletName = "PowerBIRestMethod";

        #region Constructors
        public InvokePowerBIRestMethod() : base() { }

        public InvokePowerBIRestMethod(IPowerBICmdletInitFactory init) : base(init) { }
        #endregion

        #region Parameters
        [Parameter(Mandatory = true)]
        public string Url { get; set; }

        [Parameter(Mandatory = true)]
        public PowerBIWebRequestMethod Method { get; set; }

        [Parameter(Mandatory = false)]
        public string Body { get; set; }

        [Parameter(Mandatory = false)]
        public string Organization = "myorg";

        [Parameter(Mandatory = false)]
        public string Version = "v1.0";

        [Parameter(Mandatory = false)]
        public virtual string OutFile { get; set; }

        [Parameter(Mandatory = false)]
        public string ContentType { get; set; }

        [Parameter(Mandatory = false)]
        public Hashtable Headers { get; set; }

        [Parameter(Mandatory = false)]
        public int? TimeoutSec { get; set; }
        #endregion

        public override void ExecuteCmdlet()
        {
            if(string.IsNullOrWhiteSpace(this.ContentType))
            {
                this.ContentType = "application/json";
            }

            if(!string.IsNullOrEmpty(this.OutFile))
            {
                this.OutFile = this.ResolveFilePath(this.OutFile, false);
                if(File.Exists(this.OutFile))
                {
                    this.Logger.ThrowTerminatingError(new NotSupportedException($"OutFile '{this.OutFile}' already exists, specify a new file path"), ErrorCategory.InvalidArgument);
                }
            }

            if(Uri.TryCreate(this.Url, UriKind.Absolute, out Uri testUri))
            {
                this.Url = testUri.AbsoluteUri;
            }
            else
            {
                this.Url = $"{this.Version}/{this.Organization}/" + this.Url;
            }

            if((this.Body == null) && (this.Method == PowerBIWebRequestMethod.Patch || this.Method == PowerBIWebRequestMethod.Post))
            {
                this.Logger.WriteWarning($"The {nameof(this.Body)} parameter was null, the request may be invalid when {nameof(this.Method)} parameter is {this.Method}.");
                this.Body = string.Empty;
            }
            
            var response = this.InvokeRestMethod(this.Url, this.Body, this.Method).Result;
            if (string.IsNullOrEmpty(this.OutFile))
            {
                var result = response.Content;
                if (result != null)
                {
                    this.Logger.WriteObject(result);
                }
            }
            else
            {
                using (var fileStream = new FileStream(this.OutFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    using (var responseStream = response.ContentStream)
                    {
                        responseStream.CopyTo(fileStream);
                    }
                }

                this.Logger.WriteVerbose($"OutFile '{this.OutFile}' created");
            }
        }

        private async Task<HttpResult> InvokeRestMethod(string url, string body, PowerBIWebRequestMethod requestType)
        {
            // https://msdn.microsoft.com/en-us/library/mt243842.aspx
            // https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client
            var token = this.Authenticator.Authenticate(this.Profile, this.Logger, this.Settings).Result;
            using (var client = new HttpClient())
            {
                this.PopulateClient(token, client);
                HttpResponseMessage response = null;
                if (string.IsNullOrEmpty(this.OutFile))
                {
                    switch (requestType)
                    {
                        case PowerBIWebRequestMethod.Get:
                            response = await client.GetAsync(url);
                            break;
                        case PowerBIWebRequestMethod.Post:
                            // Don't pass Content-Type header as mediaType arg in StringContent as this goes through
                            // MediaTypeHeaderValue which fails to validate boundary specifications for multipart/form types
                            HttpContent content = new StringContent(body, Encoding.UTF8);
                            content.Headers.Add("Content-Type", this.ContentType);
                            response = await client.PostAsync(url, content);
                            break;
                        case PowerBIWebRequestMethod.Delete:
                            response = await client.DeleteAsync(url);
                            break;
                        case PowerBIWebRequestMethod.Put:
                            // Don't pass Content-Type header as mediaType arg in StringContent as this goes through
                            // MediaTypeHeaderValue which fails to validate boundary specifications for multipart/form types
                            HttpContent content = new StringContent(body, Encoding.UTF8);
                            content.Headers.Add("Content-Type", this.ContentType);
                            response = await client.PutAsync(url, content);
                            break;
                        case PowerBIWebRequestMethod.Patch:
                            // Don't pass Content-Type header as mediaType arg in StringContent as this goes through
                            // MediaTypeHeaderValue which fails to validate boundary specifications for multipart/form types
                            HttpContent content = new StringContent(body, Encoding.UTF8);
                            content.Headers.Add("Content-Type", this.ContentType);
                            response = await client.PatchAsync(url, content);
                            break;
                        case PowerBIWebRequestMethod.Options:
                            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Options, url));
                            break;
                        default:
                            throw new NotSupportedException($"{nameof(requestType)} of value {requestType} is not supported");
                    }
                }
                else
                {
                    // Stream based, OutFile specified
                    HttpRequestMessage request = null;
                    switch (requestType)
                    {
                        case PowerBIWebRequestMethod.Get:
                            request = new HttpRequestMessage(HttpMethod.Get, url);
                            break;
                        case PowerBIWebRequestMethod.Post:
                            request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent(body, Encoding.UTF8, this.ContentType) };
                            break;
                        case PowerBIWebRequestMethod.Delete:
                            request = new HttpRequestMessage(HttpMethod.Delete, url);
                            break;
                        case PowerBIWebRequestMethod.Put:
                            request = new HttpRequestMessage(HttpMethod.Put, url) { Content = new StringContent(body, Encoding.UTF8, this.ContentType) };
                            break;
                        case PowerBIWebRequestMethod.Patch:
                            request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = new StringContent(body, Encoding.UTF8, this.ContentType) };
                            break;
                        case PowerBIWebRequestMethod.Options:
                            request = new HttpRequestMessage(HttpMethod.Options, url);
                            break;
                        default:
                            throw new NotSupportedException($"{nameof(requestType)} of value {requestType} is not supported");
                    }

                    response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                }

                this.Logger.WriteVerbose($"Request Uri: {response.RequestMessage.RequestUri}");
                this.Logger.WriteVerbose($"Status Code: {response.StatusCode} ({(int)response.StatusCode})");

                response.EnsureSuccessStatusCode();

                // Need to stream results back before HttpClient is disposed
                var result = new HttpResult()
                {
                    ResponseMessage = response
                };

                if (string.IsNullOrEmpty(this.OutFile))
                {
                    result.Content = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    result.ContentStream = memoryStream;
                }

                return result;
            }
        }

        protected virtual void PopulateClient(IAccessToken token, HttpClient client)
        {
            client.BaseAddress = new Uri(this.Profile.Environment.GlobalServiceEndpoint);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MicrosoftPowerBIMgmt-InvokeRest", PowerBICmdlet.CmdletVersion));

            if (this.TimeoutSec != null)
            {
                if (this.TimeoutSec < 0)
                {
                    this.Logger.ThrowTerminatingError($"{nameof(this.TimeoutSec)} value cannot be negative.");
                }
                else if (this.TimeoutSec > 0)
                {
                    client.Timeout = TimeSpan.FromSeconds(Convert.ToDouble(this.TimeoutSec));
                }
                else
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                }
            }

            if (this.Headers != null)
            {
                foreach (DictionaryEntry header in this.Headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key.ToString(), header.Value.ToString());
                }
            }
        }

        private class HttpResult
        {
            public HttpResponseMessage ResponseMessage { get; set; }
            public string Content { get; set; }
            public Stream ContentStream { get; set; }
        }
    }
}
