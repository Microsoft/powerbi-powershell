/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.PowerBI.Commands.Common
{
    [DataContract]
    public class GSEnvironments
    {
        [DataMember(Name = "environments")]
        public IEnumerable<GSEnvironment> Environments { get; set; }
    }

    [DataContract]
    public class GSEnvironment
    {
        [DataMember(Name = "cloudName")]
        public string CloudName { get; set; }

        [DataMember(Name = "services")]
        public IEnumerable<GSEnvironmentService> Services { get; set; }

        [DataMember(Name = "clients")]
        public IEnumerable<GSEnvironmentClient> Clients { get; set; }
    }

    [DataContract]
    public class GSEnvironmentService
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "endpoint")]
        public string Endpoint { get; set; }

        [DataMember(Name = "resourceId")]
        public string ResourceId { get; set; }

        [DataMember(Name = "allowedDomains")]
        public IEnumerable<string> AllowedDomains { get; set; }
    }

    [DataContract]
    public class GSEnvironmentClient
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "appId")]
        public string AppId { get; set; }

        [DataMember(Name = "redirectUri")]
        public string RedirectUri { get; set; }

        [DataMember(Name = "appInsightsId")]
        public string AppInsightsId { get; set; }

        [DataMember(Name = "localyticsId")]
        public string LocalyticsId { get; set; }
    }
}
