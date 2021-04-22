﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.PowerBI.Common.Api.Reports
{
    public interface IReportsClient
    {
        IEnumerable<Report> GetReports();

        IEnumerable<Report> GetReportsAsAdmin(string filter = default, int? top = default, int? skip = default);

        IEnumerable<Report> GetReportsForWorkspace(Guid workspaceId);

        IEnumerable<Report> GetReportsAsAdminForWorkspace(Guid workspaceId, string filter = default, int? top = default, int? skip = default);

        Stream ExportReport(Guid reportId, Guid? workspaceId = default);

        IEnumerable<Dashboard> GetDashboards();

        IEnumerable<Dashboard> GetDashboardsAsAdmin(string filter = default, int? top = default, int? skip = default);

        IEnumerable<Dashboard> GetDashboardsForWorkspace(Guid workspaceId);

        IEnumerable<Dashboard> GetDashboardsAsAdminForWorkspace(Guid workspaceId, string filter = default, int? top = default, int? skip = default);

        IEnumerable<Tile> GetTiles(Guid dashboardId);

        IEnumerable<Tile> GetTilesAsAdmin(Guid dashboardId);

        IEnumerable<Tile> GetTilesForWorkspace(Guid workspaceId, Guid dashboardId);

        Import GetImport(Guid importId);

        Import GetImportForWorkspace(Guid workspaceId, Guid importId);

        object DeleteReport(Guid reportID);

        object DeleteReport(Guid workspaceID, Guid reportID);

        IEnumerable<Import> GetImports();

        IEnumerable<Import> GetImportsAsAdmin(string expand = default, string filter = default, int? top = default, int? skip = default);

        IEnumerable<Import> GetImportsForWorkspace(Guid workspaceId);

        Guid PostImport(string datasetDisplayName, string filePath, ImportConflictHandlerModeEnum nameConflict);

        Guid PostImportForWorkspace(Guid workspaceId, string datasetDisplayName, string filePath, ImportConflictHandlerModeEnum nameConflict);

        Report PostReport(string reportName, string filePath, ImportConflictHandlerModeEnum nameConflict, int timeout);

        Report PostReportForWorkspace(Guid workspaceId, string reportName, string filePath, ImportConflictHandlerModeEnum nameConflict, int timeout);

        Report CopyReport(string reportName, string sourceWorkspaceId, string sourceReportId, string targetWorkspaceId, string targetDatasetId);

        Dashboard AddDashboard(string dashboardName, Guid workspaceId);

        Tile CopyTile(Guid workspaceId, string dashboardKey, string tileKey, string targetDashboardId, string targetWorkspaceId, string targetReportId, string targetModelId, string positionConflictAction);
    }
}
