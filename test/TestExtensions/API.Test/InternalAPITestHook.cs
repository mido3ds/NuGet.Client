// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.PackageManagement;
using NuGet.VisualStudio;
using Task = System.Threading.Tasks.Task;

namespace API.Test
{
    public static class InternalAPITestHook
    {
        private static int _cacheUpdateEventCount = 0;
        private static Action<object, NuGetEventArgs<string>> _cacheUpdateEventHandler = delegate (object sender, NuGetEventArgs<string> e)
        {
            _cacheUpdateEventCount++;
        };

        public static int CacheUpdateEventCount
        {
            get
            {
                return _cacheUpdateEventCount;
            }

            set
            {
                _cacheUpdateEventCount = value;
            }
        }

        public static void InstallLatestPackageApi(string id, bool prerelease)
        {
            var dte = ServiceLocator.GetDTE();
            var services = ServiceLocator.GetComponent<IVsPackageInstaller2>();

            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                services.InstallLatestPackage(null, project, id, prerelease, false);
                return;
            }
        }

        public static void InstallPackageApi(string id, string version)
        {
            InstallPackageApi(null, id, version, false);
        }

        public static void InstallPackageApi(string source, string id, string version, bool prerelease)
        {
            var dte = ServiceLocator.GetDTE();
            var services = ServiceLocator.GetComponent<IVsPackageInstaller>();

            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                services.InstallPackage(source, project, id, version, prerelease);
                return;
            }
        }

        public static void InstallPackageApiBadSource(string id, string version)
        {
            var dte = ServiceLocator.GetDTE();
            var services = ServiceLocator.GetComponent<IVsPackageInstaller>();

            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                services.InstallPackage("http://packagesource", project, id, version, false);
                return;
            }
        }

        public static void UninstallPackageApi(string id, bool dependency)
        {
            var dte = ServiceLocator.GetDTE();
            var uninstaller = ServiceLocator.GetComponent<IVsPackageUninstaller>();

            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                uninstaller.UninstallPackage(project, id, dependency);
                return;
            }
        }

        public static void RestorePackageApi()
        {
            var dte = ServiceLocator.GetDTE();
            var restorer = ServiceLocator.GetComponent<IVsPackageRestorer>();

            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                restorer.RestorePackages(project);
                return;
            }
        }

        public static IVsPathContext GetVsPathContext(string projectUniqueName)
        {
            var provider = ServiceLocator.GetComponent<IVsPathContextProvider>();

            IVsPathContext context;
            if (provider.TryCreateContext(projectUniqueName, out context))
            {
                return context;
            }

            return null;
        }

        public static bool ExecuteInitScript(string id, string version)
        {
            var scriptExecutor = ServiceLocator.GetComponent<IVsGlobalPackagesInitScriptExecutor>();
            // It is important that this method does not wait on ExecuteInitScriptAsync on the calling thread.
            // Calling thread is powershell execution thread and ExecuteInitScriptAsync needs to switch to
            // Powershell execution thread to execute the scripts
            var task = Task.Run(async () => await scriptExecutor.ExecuteInitScriptAsync(id, version));
            Task.WaitAny(task, Task.Delay(30000));
            if (task.IsCompleted)
            {
                return task.Result;
            }

            return false;
        }

        public static bool BatchEventsApi(string id, string version)
        {
            var dte = ServiceLocator.GetDTE();
            var packageProjectEventService = ServiceLocator.GetComponent<IVsPackageInstallerProjectEvents>();
            var installerServices = ServiceLocator.GetComponent<IVsPackageInstaller>();
            var batchStartIds = new List<string>();
            var batchEndIds = new List<string>();

            packageProjectEventService.BatchStart += (args) =>
            {
                batchStartIds.Add(args.BatchId);
            };

            packageProjectEventService.BatchEnd += (args) =>
            {
                batchEndIds.Add(args.BatchId);
            };

            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                installerServices.InstallPackage(null, project, id, version, false);
            }

            return batchStartIds.Count == 1 &&
                   batchEndIds.Count == 1 &&
                   batchStartIds[0].Equals(batchEndIds[0], StringComparison.Ordinal);
        }

        public static void ProjectCacheEventApi_AttachHandler()
        {
            var vsSolutionManager = ServiceLocator.GetComponent<ISolutionManager>();
            vsSolutionManager.AfterNuGetCacheUpdated += _cacheUpdateEventHandler.Invoke;
        }


        public static void ProjectCacheEventApi_InstallPackage(string id, string version)
        {
            var dte = ServiceLocator.GetDTE();
            var vsSolutionManager = ServiceLocator.GetComponent<ISolutionManager>();
            var installerServices = ServiceLocator.GetComponent<IVsPackageInstaller>();
            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                installerServices.InstallPackage(null, project, id, version, false);
            }
        }

        public static void ProjectCacheEventApi_DetachHandler()
        {
            var vsSolutionManager = ServiceLocator.GetComponent<ISolutionManager>();
            vsSolutionManager.AfterNuGetCacheUpdated -= _cacheUpdateEventHandler.Invoke;
        }

        public static async Task<IVsProjectJsonToPackageReferenceMigrateResult> MigrateJsonProject(string projectName)
        {
            var migrator = ServiceLocator.GetComponent<IVsProjectJsonToPackageReferenceMigrator>();
            return (IVsProjectJsonToPackageReferenceMigrateResult)await migrator.MigrateProjectJsonToPackageReferenceAsync(projectName);
        }
    }
}
