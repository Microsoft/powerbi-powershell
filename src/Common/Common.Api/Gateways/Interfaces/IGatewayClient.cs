﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.PowerBI.Common.Api.Gateways.Entities;
using System.Net.Http;

namespace Microsoft.PowerBI.Common.Api.Gateways.Interfaces
{
    public interface IGatewayClient
    {
        Task<IEnumerable<GatewayCluster>> GetGatewayClusters(bool asIndividual);
        Task<GatewayCluster> GetGatewayClusters(Guid gatewayClusterId, bool asIndividual);
        Task<GatewayClusterStatusResponse> GetGatewayClusterStatus(Guid gatewayClusterId, bool asIndividual);
        Task<HttpResponseMessage> PatchGatewayCluster(Guid gatewayClusterId, PatchGatewayClusterRequest patchGatewayClusterRequest, bool asIndividual);
        Task<HttpResponseMessage> DeleteGatewayCluster(Guid gatewayClusterId, bool asIndividual);
        Task<HttpResponseMessage> DeleteGatewayClusterMember(Guid gatewayClusterId, Guid memberGatewayId, bool asIndividual);
        Task<HttpResponseMessage> AddUsersToGatewayCluster(Guid gatewayClusterId, GatewayClusterAddPrincipalRequest addPrincipalRequest, bool asIndividual);
        Task<HttpResponseMessage> DeleteUserOnGatewayCluster(Guid gatewayClusterId, Guid permissionId, bool asIndividual);
        Task<GatewayTenant> GetTenantPolicy();
        Task<HttpResponseMessage> UpdateTenantPolicy(UpdateGatewayPolicyRequest request);
        Task<IEnumerable<InstallerPrincipal>> GetInstallerPrincipals(GatewayType? type);
        Task<HttpResponseMessage> UpdateInstallerPrincipals(UpdateGatewayInstallersRequest request);
    }
}
