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
    public class GetOnPremisesDataGatewayInstallerTests
    {
        private static CmdletInfo GetOnPremisesDataGatewayInstallerInfo { get; } = new CmdletInfo(
            $"{GetOnPremisesDataGatewayInstaller.CmdletVerb}-{GetOnPremisesDataGatewayInstaller.CmdletName}",
            typeof(GetOnPremisesDataGatewayInstaller));

        [TestMethod]
        [TestCategory("Interactive")]
        [TestCategory("SkipWhenLiveUnitTesting")] // Ignore for Live Unit Testing
        public void EndToEndGetOnPremisesDataGatewayInstaller()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ProfileTestUtilities.ConnectToPowerBI(ps);
                ps.AddCommand(GetOnPremisesDataGatewayInstallerInfo);

                // Act
                var result = ps.Invoke();

                // Assert
                TestUtilities.AssertNoCmdletErrors(ps);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void GetOnPremisesDataGatewayInstallerReturnsExpectedResults()
        {
            // Arrange
            var principalObjectId = Guid.NewGuid().ToString();
            var gatewayTypeParameter = GatewayType.Personal;
            var expectedResponse = new InstallerPrincipal {
                PrincipalObjectId = principalObjectId,
                GatewayType = gatewayTypeParameter.ToString()
            };


            var client = new Mock<IPowerBIApiClient>();
            client.Setup(x => x.Gateways
                .GetInstallerPrincipals(gatewayTypeParameter))
                .ReturnsAsync(new[] { expectedResponse });

            var initFactory = new TestPowerBICmdletInitFactory(client.Object);
            var cmdlet = new GetOnPremisesDataGatewayInstaller(initFactory)
            {
                GatewayTypeParameter = gatewayTypeParameter
            };

            // Act
            cmdlet.InvokePowerBICmdlet();

            // Assert
            TestUtilities.AssertExpectedUnitTestResults(expectedResponse, client, initFactory);
        }
    }
}

