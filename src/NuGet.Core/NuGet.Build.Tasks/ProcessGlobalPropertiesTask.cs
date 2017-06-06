// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using NuGet.Commands;
using NuGet.Common;
using NuGet.Configuration;

namespace NuGet.Build.Tasks
{
    /// <summary>
    /// Get all the settings to be used for project restore.
    /// </summary>
    public class ProcessGlobalPropertiesTask : Task
    {

        public string[] RestoreSources { get; set; }

        public string RestorePackagesPath { get; set; }

        public string[] RestoreFallbackFolders { get; set; }

        [Output]
        public bool IsRestoreSourcesGlobal { get; set; }
        [Output]
        public bool IsRestorePackagesPathGlobal { get; set; }
        [Output]
        public bool IsRestoreFallbackFoldersGlobal { get; set; }

        
        public override bool Execute()
        {
            // Log Inputs
            var log = new MSBuildLogger(Log);
            if (RestoreSources != null)
            {
                log.LogDebug($"(in) RestoreSources '{string.Join(";", RestoreSources.Select(p => p))}'");
            }
            if (RestorePackagesPath != null)
            {
                log.LogDebug($"(in) RestorePackagesPath '{RestorePackagesPath}'");
            }
            if (RestoreFallbackFolders != null)
            {
                log.LogDebug($"(in) RestoreFallbackFolders '{string.Join(";", RestoreFallbackFolders.Select(p => p))}'");
            }

            try
            {
                IsRestorePackagesPathGlobal = RestorePackagesPath != null;
                IsRestoreSourcesGlobal = RestoreSources != null;
                IsRestoreFallbackFoldersGlobal = RestoreSources != null;
            }
            catch (Exception ex)
            {
                // Log exceptions with error codes if they exist.
                ExceptionUtilities.LogException(ex, log);
                return false;
            }

            // Log Outputs
            log.LogDebug($"(out) IsRestorePackagesPathGlobal '{IsRestorePackagesPathGlobal}'");
            log.LogDebug($"(out) IsRestoreSourcesGlobal '{IsRestoreSourcesGlobal}'");
            log.LogDebug($"(out) IsRestoreFallbackFoldersGlobal '{IsRestoreFallbackFoldersGlobal}'");

            return true;
        }
    }
}