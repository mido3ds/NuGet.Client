// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Management.Automation;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using NuGet.VisualStudio;

namespace API.Test.Cmdlets
{
    public class PackageView
    {
        public string Id { get; }
        public string Version { get; }

        public PackageView(string id, string version)
        {
            Id = id;
            Version = version;
        }
    }

    [Cmdlet(VerbsCommon.Get, "InstalledPackage")]
    [OutputType(typeof(PackageView))]
    public sealed class GetInstalledPackageCommand : Cmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ProjectName { get; set; }

        protected override void ProcessRecord()
        {
            IEnumerable<IVsPackageMetadata> packages;

            if (string.IsNullOrEmpty(ProjectName))
            {
                var services = ServiceLocator.GetComponent<IVsPackageInstallerServices>();
                packages = services.GetInstalledPackages();
            }
            else
            {
                packages = GetInstalledPackagesForProject();
            }

            foreach (var package in packages)
            {
                WriteObject(new PackageView(package.Id, package.VersionString), enumerateCollection: true);
            }
        }

        private IEnumerable<IVsPackageMetadata> GetInstalledPackagesForProject()
        {
            return ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var dteSolution = await VSSolutionHelper.GetDTESolutionAsync();
                var project = await VSSolutionHelper.GetProjectAsync(dteSolution, ProjectName);
                Assumes.Present(project);

                var services = ServiceLocator.GetComponent<IVsPackageInstallerServices>();
                return services.GetInstalledPackages(project);
            });
        }
    }
}
