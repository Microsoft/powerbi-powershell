﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System.Management.Automation;
using Microsoft.PowerBI.Common.Api.Gateways.Entities;
using Microsoft.PowerBI.Common.Client;

namespace Microsoft.PowerBI.Commands.OnPremisesDataGateway
{
    [Cmdlet(CmdletVerb, CmdletName)]
    [OutputType(typeof(InstallerPrincipal))]
    public class GetOnPremisesDataGatewayInstaller : PowerBIClientCmdlet
    {
        public const string CmdletName = "OnPremisesDataGatewayInstaller";
        public const string CmdletVerb = VerbsCommon.Get;

        [Parameter()]
        public GatewayType? GatewayTypeParameter { get; set; } = null;

        public GetOnPremisesDataGatewayInstaller() : base() { }

        public GetOnPremisesDataGatewayInstaller(IPowerBIClientCmdletInitFactory init) : base(init) { }

        public override void ExecuteCmdlet()
        {
            using (var client = CreateClient())
            {
                var gatewayInstallerPrincipals = client.Gateways.GetInstallerPrincipals(GatewayTypeParameter).Result;
                Logger.WriteObject(gatewayInstallerPrincipals, true);
            }
        }
    }
}
