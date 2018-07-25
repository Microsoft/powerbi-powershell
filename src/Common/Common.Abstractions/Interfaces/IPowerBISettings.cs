﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System.Collections.Generic;

namespace Microsoft.PowerBI.Common.Abstractions.Interfaces
{
    /// <summary>
    /// PowerBI default settings.
    /// </summary>
    public interface IPowerBISettings
    {
        /// <summary>
        /// Available PowerBI environment settings to pick from.
        /// </summary>
        IDictionary<PowerBIEnvironmentType, IPowerBIEnvironment> Environments { get; }

        /// <summary>
        /// Power BI configuration settings.
        /// </summary>
        IPowerBIConfigurationSettings Settings { get; }
    }
}