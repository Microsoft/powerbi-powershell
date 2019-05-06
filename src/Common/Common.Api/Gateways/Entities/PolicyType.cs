﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System.Runtime.Serialization;

namespace Microsoft.PowerBI.Common.Api.Gateways.Entities
{
    [DataContract]
    public enum PolicyType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Open = 1,
        [EnumMember]
        Restricted = 2
    }
}
