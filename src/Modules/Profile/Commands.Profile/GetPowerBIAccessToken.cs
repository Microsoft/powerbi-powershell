﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System.Collections;
using System.Management.Automation;
using Microsoft.PowerBI.Commands.Common;
using Microsoft.PowerBI.Common.Abstractions.Interfaces;

namespace Microsoft.PowerBI.Commands.Profile
{
    [Cmdlet(CmdletVerb, CmdletName)]
    [OutputType(typeof(Hashtable), typeof(string))]
    public class GetPowerBIAccessToken : PowerBICmdlet
    {
        public const string CmdletVerb = VerbsCommon.Get;
        public const string CmdletName = "PowerBIAccessToken";

        #region Constructors
        public GetPowerBIAccessToken() : base() { }

        public GetPowerBIAccessToken(IPowerBICmdletInitFactory init) : base(init) { }
        #endregion

        #region Parameters
        [Parameter(Mandatory = false)]
        public SwitchParameter AsString { get; set; }
        #endregion

        public override void ExecuteCmdlet()
        {
            var token = this.Authenticator.Authenticate(this.Profile, this.Logger, this.Settings).Result;
            if (this.AsString.IsPresent)
            {
                this.Logger.WriteObject(token.AuthorizationHeader);
            }
            else
            {
                this.Logger.WriteObject(new Hashtable() {
                    { "Authorization", token.AuthorizationHeader }
                });
            }
            
        }
    }
}
