﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System;
using System.Management.Automation;
using Microsoft.PowerBI.Commands.Common.Test;
using Microsoft.PowerBI.Commands.Profile.Test;
using Microsoft.PowerBI.Common.Api;
using Microsoft.PowerBI.Common.Api.Gateways.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.PowerBI.Commands.OnPremisesDataGateway.Test
{
    [TestClass]
    public class GetOnPremisesDataGatewayClusterTests
    {
        private static CmdletInfo GetOnPremisesDataGatewayClusterInfo { get; } = new CmdletInfo(
            $"{GetOnPremisesDataGatewayCluster.CmdletVerb}-{GetOnPremisesDataGatewayCluster.CmdletName}",
            typeof(GetOnPremisesDataGatewayCluster));

        [TestMethod]
        [TestCategory("Interactive")]
        [TestCategory("SkipWhenLiveUnitTesting")] // Ignore for Live Unit Testing
        public void EndToEndGetOnPremisesDataGatewayCluster()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ProfileTestUtilities.ConnectToPowerBI(ps);
                ps.AddCommand(GetOnPremisesDataGatewayClusterInfo);

                // Act
                var result = ps.Invoke();

                // Assert
                TestUtilities.AssertNoCmdletErrors(ps);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [TestCategory("Interactive")]
        [TestCategory("SkipWhenLiveUnitTesting")] // Ignore for Live Unit Testing
        public void EndToEndGetOnPremisesDataGatewayClusterWithGatewayClusterId()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ProfileTestUtilities.ConnectToPowerBI(ps);
                ps.AddCommand(GetOnPremisesDataGatewayClusterInfo).AddParameter(nameof(GetOnPremisesDataGatewayCluster.GatewayClusterId), Guid.NewGuid());

                // Act
                var result = ps.Invoke();

                // Assert
                TestUtilities.AssertNoCmdletErrors(ps);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void GetOnPremisesDataGatewayClusterReturnsExpectedResults()
        {
            // Arrange
            var clusterId = Guid.NewGuid();
            var expectedResponse = new GatewayCluster
            {
                Id = clusterId,
                Name = "the cluster name",
                Description = "the cluster description",
                Status = "the status",
                Region = "the region",
                Permissions = new Permission[]
                {
                    new Permission
                    {
                        Id = Guid.NewGuid().ToString(),
                        PrincipalType = "User",
                        TenantId = Guid.NewGuid().ToString(),
                        Role = "Admin",
                        AllowedDataSources = new string[]
                        {
                            Guid.NewGuid().ToString()
                        },
                        ClusterId = clusterId.ToString()
                    }
                },
                DataSourceIds = new Guid[]
                {
                    Guid.NewGuid()
                },
                Type = "the type",
                MemberGateways = new MemberGateway[]
                {
                    new MemberGateway
                    {
                        Id = clusterId,
                        Name = "the member name",
                        Description = "the member description",
                        Status = "the member status",
                        Region = "the member region",
                        Type = "the member type",
                        Version = "the version",
                        Annotation = "the annotation",
                        ClusterId = clusterId
                    }
                }
            };

            var client = new Mock<IPowerBIApiClient>();
            client.Setup(x => x.Gateways
                .GetGatewayClusters(clusterId, true))
                .ReturnsAsync(expectedResponse);

            var initFactory = new TestPowerBICmdletInitFactory(client.Object);
            var cmdlet = new GetOnPremisesDataGatewayCluster(initFactory)
            {
                GatewayClusterId = clusterId
            };

            // Act
            cmdlet.InvokePowerBICmdlet();

            // Assert
            TestUtilities.AssertExpectedUnitTestResults(expectedResponse, client, initFactory);
        }

        [TestMethod]
        public void GetOnPremisesDataGatewayClusterSingleClusterReturnsExpectedResults()
        {
            // Arrange
            var clusterId = Guid.NewGuid();
            var expectedResponse = new GatewayCluster
            {
                Id = clusterId,
                Name = "the cluster name",
                Description = "the cluster description",
                Status = "the status",
                Region = "the region",
                Permissions = new Permission[]
                {
                    new Permission
                    {
                        Id = Guid.NewGuid().ToString(),
                        PrincipalType = "User",
                        TenantId = Guid.NewGuid().ToString(),
                        Role = "Admin",
                        AllowedDataSources = new string[]
                        {
                            Guid.NewGuid().ToString()
                        },
                        ClusterId = clusterId.ToString()
                    }
                },
                DataSourceIds = new Guid[]
                {
                    Guid.NewGuid()
                },
                Type = "the type",
                MemberGateways = new MemberGateway[]
                {
                    new MemberGateway
                    {
                        Id = clusterId,
                        Name = "the member name",
                        Description = "the member description",
                        Status = "the member status",
                        Region = "the member region",
                        Type = "the member type",
                        Version = "the version",
                        Annotation = "the annotation",
                        ClusterId = clusterId
                    }
                }
            };

            var client = new Mock<IPowerBIApiClient>();
            client.Setup(x => x.Gateways
                .GetGatewayClusters(true))
                .ReturnsAsync(new[] { expectedResponse });

            var initFactory = new TestPowerBICmdletInitFactory(client.Object);
            var cmdlet = new GetOnPremisesDataGatewayCluster(initFactory);

            // Act
            cmdlet.InvokePowerBICmdlet();

            // Assert
            TestUtilities.AssertExpectedUnitTestResults(expectedResponse, client, initFactory);
        }
    }
}
