/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Linq;
using Microsoft.PowerBI.Common.Abstractions;
using Microsoft.PowerBI.Common.Abstractions.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.PowerBI.Commands.Common.Test
{
    [TestClass]
    public class PowerBISettingsTests
    {
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?tabs=basicconfiguration

        [TestMethod]
        public void InValidEnvironmentInJson()
        {
            var testJson = @"
{
  ""Environments"": [
    {
      ""name"": ""Public"",
      ""authority"": ""https://login.windows-ppe.net/common/oauth2/authorize"",
      ""clientId"": ""ea0616ba-638b-4df5-95b9-636659ae5121"",
      ""redirect"": ""https://login.microsoftonline.com/common/oauth2/nativeclient"",
      ""resource"": ""https://analysis.windows-int.net/powerbi/api"",
      ""globalService"": ""https://api.powerbi.com"",
      ""redirectApp"": ""powerbi-powershell""
    },
{
      ""name"": ""NotValid"",
      ""authority"": ""https://login.windows-ppe.net/common/oauth2/authorize"",
      ""clientId"": ""ea0616ba-638b-4df5-95b9-636659ae5121"",
      ""redirect"": ""https://login.microsoftonline.com/common/oauth2/nativeclient"",
      ""resource"": ""https://analysis.windows-int.net/powerbi/api"",
      ""globalService"": ""https://api.powerbi.com"",
      ""redirectApp"": ""powerbi-powershell""
    }
  ],
  ""Settings"": {
    ""ForceDeviceCodeAuthentication"": true
  }
}
";

            var testDir = Directory.GetCurrentDirectory();

            File.WriteAllText(Path.Combine(testDir, "testSettings.json"), testJson);

            var settings = new PowerBISettings(Path.Combine(testDir, "testSettings.json"));
            Assert.IsNotNull(settings.Environments);
            Assert.IsNotNull(settings.Settings);
            Assert.IsTrue(settings.Environments.Count == 1);
            Assert.AreEqual(true, settings.Settings.ForceDeviceCodeAuthentication);
        }

        [TestMethod]
        public void ReadDefaultSettingsFile_NonCloudEnvironments()
        {
            var settings = new PowerBISettings();

            Assert.IsNotNull(settings.Environments);
            Assert.IsNotNull(settings.Settings);
            Assert.IsTrue(settings.Environments.Any());

#if DEBUG
            var oneBoxEnvironment = settings.Environments[PowerBIEnvironmentType.OneBox];
            Assert.AreEqual(PowerBIEnvironmentType.OneBox, oneBoxEnvironment.Name);
            Assert.AreEqual("https://login.windows-ppe.net/common/oauth2/authorize", oneBoxEnvironment.AzureADAuthority);
            AssertValidEnvironmentSharedProperties(oneBoxEnvironment);
            Assert.AreEqual("https://analysis.windows-int.net/powerbi/api", oneBoxEnvironment.AzureADResource);
            Assert.AreEqual("https://onebox-redirect.analysis.windows-int.net", oneBoxEnvironment.GlobalServiceEndpoint);

            var edogEnvironment = settings.Environments[PowerBIEnvironmentType.EDog];
            Assert.AreEqual(PowerBIEnvironmentType.EDog, edogEnvironment.Name);
            Assert.AreEqual("https://login.windows-ppe.net/common/oauth2/authorize", edogEnvironment.AzureADAuthority);
            AssertValidEnvironmentSharedProperties(edogEnvironment);
            Assert.AreEqual("https://analysis.windows-int.net/powerbi/api", edogEnvironment.AzureADResource);
            Assert.AreEqual("https://biazure-int-edog-redirect.analysis-df.windows.net", edogEnvironment.GlobalServiceEndpoint);

            var dxtEnvironment = settings.Environments[PowerBIEnvironmentType.DXT];
            Assert.AreEqual(PowerBIEnvironmentType.DXT, dxtEnvironment.Name);
            Assert.AreEqual("https://login.microsoftonline.com/common/oauth2/authorize", dxtEnvironment.AzureADAuthority);
            AssertValidEnvironmentSharedProperties(dxtEnvironment);
            Assert.AreEqual("https://analysis.windows.net/powerbi/api", dxtEnvironment.AzureADResource);
            Assert.AreEqual("https://wabi-staging-us-east-redirect.analysis.windows.net", dxtEnvironment.GlobalServiceEndpoint);
#endif
        }

        [TestMethod]
        public void ReadDefaultSettingsFile_CloudEnvironments()
        {
            var settings = new PowerBISettings();
            var cloudEnvironments = settings.GetGlobalServiceConfig().Result;

            Assert.IsNotNull(settings.Environments);
            Assert.IsNotNull(settings.Settings);
            Assert.IsTrue(settings.Environments.Any());

            var publicEnvironment = settings.Environments[PowerBIEnvironmentType.Public];
            Assert.AreEqual(PowerBIEnvironmentType.Public, publicEnvironment.Name);
            AssertValidCloudEnvironment("GlobalCloud", publicEnvironment, cloudEnvironments);
        }

        private static void AssertValidCloudEnvironment(string cloudName, IPowerBIEnvironment environment, GSEnvironments cloudEnvironments)
        {
            var cloudEnvironment = cloudEnvironments.Environments.FirstOrDefault(c => c.CloudName.Equals(cloudName, StringComparison.OrdinalIgnoreCase));
            var backendService = cloudEnvironment.Services.First(s => s.Name.Equals("powerbi-backend", StringComparison.OrdinalIgnoreCase));
            var redirectApp = cloudEnvironment.Clients.First(s => s.Name.Equals("powerbi-gateway", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(cloudEnvironment.Services.First(s => s.Name.Equals("aad", StringComparison.OrdinalIgnoreCase)).Endpoint, environment.AzureADAuthority);
            AssertValidEnvironmentSharedProperties(environment);
            Assert.AreEqual(backendService.ResourceId, environment.AzureADResource);
            Assert.AreEqual(backendService.Endpoint, environment.GlobalServiceEndpoint);
            Assert.AreEqual(redirectApp.AppId, environment.AzureADClientId);
            Assert.AreEqual(redirectApp.RedirectUri, environment.AzureADRedirectAddress);
        }

        private static void AssertValidEnvironmentSharedProperties(IPowerBIEnvironment environment)
        {
            Assert.AreEqual("23d8f6bd-1eb0-4cc2-a08c-7bf525c67bcd", environment.AzureADClientId);
            Assert.AreEqual("https://login.microsoftonline.com/common/oauth2/nativeclient", environment.AzureADRedirectAddress);
        }
    }
}
