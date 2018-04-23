﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.PowerBI.Commands.Common.Test;
using Microsoft.PowerBI.Commands.Profile.Test;
using Microsoft.PowerBI.Common.Abstractions;
using Microsoft.PowerBI.Common.Api.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.PowerBI.Commands.Workspaces.Test
{
    [TestClass]
    public class SetPowerBIWorkspaceTests
    {
        private static CmdletInfo Cmdlet => new CmdletInfo($"{SetPowerBIWorkspace.CmdletVerb}-{SetPowerBIWorkspace.CmdletName}", typeof(SetPowerBIWorkspace));

        [TestMethod]
        [TestCategory("Interactive")]
        [TestCategory("SkipWhenLiveUnitTesting")] // Ignore for Live Unit Testing
        public void EndToEndSetWorkspaceOrganizationScopePropertiesParameterSet()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ProfileTestUtilities.ConnectToPowerBI(ps);
                var workspace = WorkspacesTestUtilities.GetFirstWorkspaceInOrganization(ps);
                WorkspacesTestUtilities.AssertShouldContinueOrganizationTest(workspace);
                var updatedName = TestUtilities.GetRandomString();
                var updatedDescription = TestUtilities.GetRandomString();
                var parameters = new Dictionary<string, object>
                {
                    { nameof(SetPowerBIWorkspace.Scope), PowerBIUserScope.Organization },
                    { nameof(SetPowerBIWorkspace.Id), workspace.Id },
                    { nameof(SetPowerBIWorkspace.Name), updatedName },
                    { nameof(SetPowerBIWorkspace.Description), updatedDescription },
                };
                ps.AddCommand(Cmdlet).AddParameters(parameters);

                // Act
                var results = ps.Invoke();

                // Assert
                TestUtilities.AssertNoCmdletErrors(ps);
                Assert.IsNotNull(results);
                var updatedWorkspace = WorkspacesTestUtilities.GetWorkspace(ps, PowerBIUserScope.Organization, workspace.Id);
                Assert.AreEqual(updatedName, updatedWorkspace.Name);
                Assert.AreEqual(updatedDescription, updatedWorkspace.Description);
            }
        }

        [TestMethod]
        [TestCategory("Interactive")]
        [TestCategory("SkipWhenLiveUnitTesting")] // Ignore for Live Unit Testing
        public void EndToEndSetWorkspaceOrganizationScopeWorkspaceParameterSet()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ProfileTestUtilities.ConnectToPowerBI(ps);
                var workspace = WorkspacesTestUtilities.GetFirstWorkspaceInOrganization(ps);
                WorkspacesTestUtilities.AssertShouldContinueOrganizationTest(workspace);
                var updatedName = TestUtilities.GetRandomString();
                var updatedDescription = TestUtilities.GetRandomString();
                workspace.Name = updatedName;
                workspace.Description = updatedDescription;
                var parameters = new Dictionary<string, object>
                {
                    { nameof(SetPowerBIWorkspace.Scope), PowerBIUserScope.Organization },
                    { nameof(SetPowerBIWorkspace.Workspace), workspace }
                };
                ps.AddCommand(Cmdlet).AddParameters(parameters);

                // Act
                var results = ps.Invoke();

                // Assert
                TestUtilities.AssertNoCmdletErrors(ps);
                Assert.IsNotNull(results);
                var updatedWorkspace = WorkspacesTestUtilities.GetWorkspace(ps, PowerBIUserScope.Organization, workspace.Id);
                Assert.AreEqual(updatedName, updatedWorkspace.Name);
                Assert.AreEqual(updatedDescription, updatedWorkspace.Description);
            }
        }

        [TestMethod]
        [TestCategory("Interactive")]
        [TestCategory("SkipWhenLiveUnitTesting")] // Ignore for Live Unit Testing
        public void EndToEndSetWorkspaceIndividualScope()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ProfileTestUtilities.ConnectToPowerBI(ps);
                var parameters = new Dictionary<string, object>
                {
                    { nameof(SetPowerBIWorkspace.Scope), PowerBIUserScope.Individual },
                    { nameof(SetPowerBIWorkspace.Workspace), new Workspace() }
                };
                ps.AddCommand(Cmdlet).AddParameters(parameters);

                try
                {
                    // Act
                    ps.Invoke();

                    Assert.Fail("Should not have reached this point");
                }
                catch (CmdletInvocationException ex)
                {
                    // Assert
                    Assert.AreEqual(ex.InnerException.GetType(), typeof(NotImplementedException));
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(CmdletInvocationException))]
        public void CallSetWorkspaceWithoutLogin()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ProfileTestUtilities.SafeDisconnectFromPowerBI(ps);
                var parameters = new Dictionary<string, object>
                {
                    { nameof(SetPowerBIWorkspace.Scope), PowerBIUserScope.Organization },
                    { nameof(SetPowerBIWorkspace.Workspace), new Workspace() }
                };
                ps.AddCommand(Cmdlet).AddParameters(parameters);

                // Act
                ps.Invoke();

                // Assert
                Assert.Fail("Should not have reached this point");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ParameterBindingException))]
        public void CallSetWorkspaceWithoutRequiredParameterIdOrWorkspace()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ps.AddCommand(Cmdlet)
                    .AddParameter(nameof(GetPowerBIWorkspace.Scope), PowerBIUserScope.Organization);

                // Act
                var results = ps.Invoke();

                // Assert
                Assert.Fail("Should not have reached this point");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ParameterBindingException))]
        public void CallSetWorkspaceWithBothParameterSets()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                var parameters = new Dictionary<string, object>
                {
                    { nameof(SetPowerBIWorkspace.Scope), PowerBIUserScope.Organization },
                    { nameof(SetPowerBIWorkspace.Id), new Guid() },
                    { nameof(SetPowerBIWorkspace.Workspace), new Workspace() }
                };
                ps.AddCommand(Cmdlet).AddParameters(parameters);

                // Act
                var results = ps.Invoke();

                // Assert
                Assert.Fail("Should not have reached this point");
            }
        }
    }
}
