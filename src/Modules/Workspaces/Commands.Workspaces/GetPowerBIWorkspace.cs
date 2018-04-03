﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.PowerBI.Common.Abstractions;
using Microsoft.PowerBI.Common.Abstractions.Interfaces;
using Microsoft.PowerBI.Common.Client;

namespace Microsoft.PowerBI.Commands.Workspaces
{
    [Cmdlet(CmdletVerb, CmdletName, DefaultParameterSetName = ListParameterSetName)]
    [Alias("Get-PowerBIGroup")]
    [OutputType(typeof(IEnumerable<Group>))]
    public class GetPowerBIWorkspace : PowerBIClientCmdlet, IUserScope, IUserFilter, IUserId
    {
        public const string CmdletName = "PowerBIWorkspace";
        public const string CmdletVerb = VerbsCommon.Get;

        private const string IdParameterSetName = "Id";
        private const string NameParameterSetName = "Name";
        private const string ListParameterSetName = "List";

        // Since internally, users are null rather than an empty list on workspaces v1 (groups), we don't need to filter on type for the time being
        private const string OrphanedFilterString = "(not users/any()) or (not users/any(u: u/groupUserAccessRight eq Microsoft.PowerBI.ServiceContracts.Api.GroupUserAccessRight'Admin'))";

        public GetPowerBIWorkspace() : base() { }

        public GetPowerBIWorkspace(IPowerBIClientCmdletInitFactory init) : base(init) { }

        #region Parameters

        [Parameter(Mandatory = true, ParameterSetName = IdParameterSetName)]
        [Alias("GroupId", "WorkspaceId")]
        public Guid Id { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = NameParameterSetName)]
        public string Name { get; set; }

        [Parameter(Mandatory = false)]
        public PowerBIUserScope Scope { get; set; } = PowerBIUserScope.Individual;

        [Parameter(Mandatory = false, ParameterSetName = ListParameterSetName)]
        public string Filter { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = ListParameterSetName)]
        public string User { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = ListParameterSetName)]
        public SwitchParameter Orphaned { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = ListParameterSetName)]
        [Alias("Top")]
        public int? First { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = ListParameterSetName)]
        public int? Skip { get; set; }


        #endregion

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (!string.IsNullOrEmpty(this.User) && this.Scope.Equals(PowerBIUserScope.Individual))
            {
                this.Logger.ThrowTerminatingError($"{nameof(this.User)} is only applied when -{nameof(this.Scope)} is set to {nameof(PowerBIUserScope.Organization)}");
            }
        }

        public override void ExecuteCmdlet()
        {
            if (this.Orphaned.IsPresent && this.Scope.Equals(PowerBIUserScope.Individual))
            {
                // You can't have orphaned workspaces when scope is individual as orphaned workspaces are unassigned
                return;
            }

            IPowerBIClient client = this.CreateClient();

            if(this.Orphaned.IsPresent)
            {
                this.Filter = string.IsNullOrEmpty(this.Filter) ? OrphanedFilterString : $"({this.Filter}) and ({OrphanedFilterString})";
            }

            if(this.ParameterSetName == IdParameterSetName)
            {
                this.Filter = $"id eq '{this.Id}'";
            }

            if (this.ParameterSetName == NameParameterSetName)
            {
                this.Filter = $"tolower(name) eq '{this.Name.ToLower()}'";
            }

            if(!string.IsNullOrEmpty(this.User) && this.Scope == PowerBIUserScope.Organization)
            {
                var userFilter = $"users/any(u: tolower(u/emailAddress) eq '{this.User.ToLower()}')";
                this.Filter = string.IsNullOrEmpty(this.Filter) ? userFilter : $"({this.Filter}) and ({userFilter})";
            }

            var workspacesResult = this.Scope == PowerBIUserScope.Individual ? 
                client.Groups.GetGroups(filter: this.Filter, top: this.First, skip: this.Skip) : 
                client.Groups.GetGroupsAsAdmin(expand: "users", filter: this.Filter, top: this.First, skip: this.Skip);
            var workspaces = workspacesResult.Value;

            this.Logger.WriteObject(workspaces, true);
        }
    }
}