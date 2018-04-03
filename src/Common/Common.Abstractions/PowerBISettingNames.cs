﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

namespace Microsoft.PowerBI.Common.Abstractions
{
    public static class PowerBISettingNames
    {
        public const string FileName = "settings.json";

        public static class SettingsSection
        {
            public const string SectionName = "Settings";

            public const string ForceDeviceCodeAuthentication = "ForceDeviceCodeAuthentication";

            public const string ShowADALDebugMessages = "ShowADALDebugMessages";
        }

        public static class Environments
        {
            public const string SectionName = "Environments";

            public const string Name = "name";
            public const string Authority = "authority";
            public const string ClientId = "clientId";
            public const string Redirect = "redirect";
            public const string Resource = "resource";
            public const string GlobalService = "globalService";
            public const string CloudName = "cloudName";
        }
    }
}