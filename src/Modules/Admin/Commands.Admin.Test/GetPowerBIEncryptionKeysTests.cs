﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.PowerBI.Commands.Common.Test;
using Microsoft.PowerBI.Commands.Profile.Test;
using Microsoft.PowerBI.Common.Abstractions;
using Microsoft.PowerBI.Common.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.PowerBI.Commands.Admin.Test
{
    [TestClass]
    public class GetPowerBIEncryptionKeysTests
    {
        private static CmdletInfo GetPowerBIEncryptionKeysCmdletInfo => new CmdletInfo($"{GetPowerBIEncryptionKeys.CmdletVerb}-{GetPowerBIEncryptionKeys.CmdletName}", typeof(GetPowerBIEncryptionKeys));

        [TestMethod]
        [TestCategory("Interactive")]
        [TestCategory("SkipWhenLiveUnitTesting")] // Ignore for Live Unit Testing
        public void EndToEndGetPowerBIEncryptionKeys()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ProfileTestUtilities.ConnectToPowerBI(ps, PowerBIEnvironmentType.OneBox);
                ps.AddCommand(GetPowerBIEncryptionKeysCmdletInfo);

                // Act
                var result = ps.Invoke();

                // Assert
                TestUtilities.AssertNoCmdletErrors(ps);
            }
        }

        [TestMethod]
        public void GetPowerBIEncryptionKeys_WithValidResponse()
        {
            // Arrange
            var tenantKey1 = new TenantKey()
            {
                Id = Guid.NewGuid(),
                Name = "KeyName1",
                KeyVaultKeyIdentifier = "KeyVaultUri1",
                IsDefault = true,
                CreatedAt = new DateTime(1995, 1, 1),
                UpdatedAt = new DateTime(1995, 1, 1)
            };
            var tenantKey2 = new TenantKey()
            {
                Id = Guid.NewGuid(),
                Name = "KeyName2",
                KeyVaultKeyIdentifier = "KeyVaultUri2",
                IsDefault = true,
                CreatedAt = new DateTime(1995, 1, 1),
                UpdatedAt = new DateTime(1995, 1, 1)
            };
            var tenantKeys = new List<TenantKey>();
            tenantKeys.Add(tenantKey1);
            tenantKeys.Add(tenantKey2);
            var client = new Mock<IPowerBIApiClient>();
            client.Setup(x => x.Admin.GetPowerBIEncryptionKeys()).Returns(tenantKeys);
            var initFactory = new TestPowerBICmdletInitFactory(client.Object);
            var cmdlet = new GetPowerBIEncryptionKeys(initFactory);

            // Act
            cmdlet.InvokePowerBICmdlet();

            // Assert
            client.Verify(x => x.Admin.GetPowerBIEncryptionKeys(), Times.Once());
        }

        [TestMethod]
        public void GetPowerBIEncryptionKeys_WithApiThrowingException()
        {
            // Arrange
            var client = new Mock<IPowerBIApiClient>();
            client.Setup(x => x.Admin.GetPowerBIEncryptionKeys()).Throws(new Exception("Some exception"));
            var initFactory = new TestPowerBICmdletInitFactory(client.Object);
            var cmdlet = new GetPowerBIEncryptionKeys(initFactory);

            // Act
            cmdlet.InvokePowerBICmdlet();

            // Assert
            var throwingErrorRecords = initFactory.Logger.ThrowingErrorRecords;
            Assert.IsTrue(throwingErrorRecords.Count() > 0, "Should throw Exception");
            Assert.AreEqual(throwingErrorRecords.First().ToString(), "Some exception");
        }
    }
}
