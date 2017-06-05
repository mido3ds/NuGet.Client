// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Management.Automation;
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

    [Cmdlet(VerbsCommon.Get, "InstalledPackages")]
    [OutputType(typeof(PackageView))]
    public sealed class GetInstalledPackagesCommand : Cmdlet
    {
        protected override void ProcessRecord()
        {
            var services = ServiceLocator.GetComponent<IVsPackageInstallerServices>();

            var packages = services.GetInstalledPackages();
            foreach (var package in packages)
            {
                WriteObject(new PackageView(package.Id, package.VersionString), true);
            }
        }
    }
}
