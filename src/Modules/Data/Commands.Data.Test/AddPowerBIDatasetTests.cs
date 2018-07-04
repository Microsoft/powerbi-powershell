/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.PowerBI.Commands.Common.Test;
using Microsoft.PowerBI.Commands.Profile.Test;
using Microsoft.PowerBI.Common.Abstractions;
using Microsoft.PowerBI.Common.Api;
using Microsoft.PowerBI.Common.Api.Datasets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

namespace Microsoft.PowerBI.Commands.Data.Test
{
    [TestClass]
    public class AddPowerBIDatasetTests
    {
        private static CmdletInfo NewPowerBIColumnCmdletInfo => new CmdletInfo($"{NewPowerBIColumn.CmdletVerb}-{NewPowerBIColumn.CmdletName}", typeof(NewPowerBIColumn));
        private static CmdletInfo NewPowerBITableCmdletInfo => new CmdletInfo($"{NewPowerBITable.CmdletVerb}-{NewPowerBITable.CmdletName}", typeof(NewPowerBITable));
        private static CmdletInfo NewPowerBIDatasetCmdletInfo => new CmdletInfo($"{NewPowerBIDataset.CmdletVerb}-{NewPowerBIDataset.CmdletName}", typeof(NewPowerBIDataset));
        private static CmdletInfo AddPowerBIDatasetCmdletInfo => new CmdletInfo($"{AddPowerBIDataset.CmdletVerb}-{AddPowerBIDataset.CmdletName}", typeof(AddPowerBIDataset));

        [TestMethod]
        [TestCategory("Interactive")]
        [TestCategory("SkipWhenLiveUnitTesting")] // Ignore for Live Unit Testing
        public void EndToEndAddPowerBIDataset()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ProfileTestUtilities.ConnectToPowerBI(ps, nameof(PowerBIEnvironmentType.Public));
                ps.AddCommand(NewPowerBIColumnCmdletInfo).AddParameter("Name", "Col1").AddParameter("DataType", PowerBIDataType.String);
                var col1 = ps.Invoke();
                ps.Commands.Clear();
                ps.AddCommand(NewPowerBIColumnCmdletInfo).AddParameter("Name", "Col2").AddParameter("DataType", PowerBIDataType.Boolean);
                var col2 = ps.Invoke();
                ps.Commands.Clear();
                var columns = new List<Column>() { col1.First().BaseObject as Column, col2.First().BaseObject as Column };
                ps.AddCommand(NewPowerBITableCmdletInfo).AddParameter("Name", "Table1").AddParameter("Columns", columns);
                var table1 = ps.Invoke();
                var tables = new List<Table>() { table1.First().BaseObject as Table };
                ps.Commands.Clear();
                ps.AddCommand(NewPowerBIDatasetCmdletInfo).AddParameter("Name", "MyDataSet").AddParameter("Tables", tables);
                var dataset = ps.Invoke();
                ps.Commands.Clear();

                ps.AddCommand(AddPowerBIDatasetCmdletInfo)
                    .AddParameter("Dataset", dataset.First().BaseObject as Dataset);
                
                // Act
                var result = ps.Invoke();

                // Assert
                TestUtilities.AssertNoCmdletErrors(ps);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count > 0);
            }
        }

        [TestMethod]
        public void AddPowerBIDataset_DatasetParameterSetName()
        {
            var expectedDataset = new Dataset { Id = Guid.NewGuid(), Name = "TestDataset" };
            var client = new Mock<IPowerBIApiClient>();
            client.Setup(x => x.Datasets.AddDataset(expectedDataset, null)).Returns(expectedDataset);
            var initFactory = new TestPowerBICmdletInitFactory(client.Object);
            var cmdlet = new AddPowerBIDataset(initFactory);

            cmdlet.Dataset = expectedDataset;

            // Act
            cmdlet.InvokePowerBICmdlet();

            // Assert
            Assert.AreEqual(expectedDataset.Name, (initFactory.Logger.Output.First() as Dataset).Name);
        }

        [TestMethod]
        public void AddPowerBIDataset_OrganizationScope()
        {
            var dataset = new Dataset();
            var client = new Mock<IPowerBIApiClient>();
            client.Setup(x => x.Datasets.AddDataset(dataset, null)).Returns(dataset);
            var initFactory = new TestPowerBICmdletInitFactory(client.Object);

            var cmdlet = new AddPowerBIDataset(initFactory);
            cmdlet.Dataset = dataset;
            cmdlet.Scope = PowerBIUserScope.Organization;

            try
            {
                // Act
                cmdlet.InvokePowerBICmdlet();

                Assert.Fail("Should not have reached this point");
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                // Assert
                Assert.AreEqual(ex.InnerException.GetType(), typeof(NotImplementedException));
            }
        }
    }    
}
