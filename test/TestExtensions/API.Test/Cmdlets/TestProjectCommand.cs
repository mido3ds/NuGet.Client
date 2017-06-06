// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Management.Automation;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace API.Test.Cmdlets
{
    [Cmdlet(VerbsDiagnostic.Test, "Project")]
    [OutputType(typeof(bool))]
    public sealed class TestProjectCommand : Cmdlet
    {
        private bool _isDeferred;

        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ProjectName { get; set; }

        [Parameter]
        public SwitchParameter IsDeferred { get => _isDeferred; set => _isDeferred = value; }

        protected override void ProcessRecord()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var dteSolution = await VSSolutionHelper.GetDTESolutionAsync();
                var project = await VSSolutionHelper.GetProjectAsync(dteSolution, ProjectName);
                if (project == null)
                {
                    WriteVerbose($"Project '{ProjectName}' is not found.");
                    WriteObject(false);
                    return;
                }

                if (IsDeferred)
                {
                    WriteObject(await TestProjectIsDeferredAsync(project));
                    return;
                }

                WriteObject(true);
            });
        }

        private static async Task<bool> TestProjectIsDeferredAsync(EnvDTE.Project project)
        {
#if VS14
            await Task.Yield();
            return false;
#else
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var solution = ServiceLocator.GetService<SVsSolution, IVsSolution>();
            ErrorHandler.ThrowOnFailure(solution.GetProjectOfUniqueName(project.UniqueName, out var hierarchy));

            object isDeferred;
            if (ErrorHandler.Failed(hierarchy.GetProperty(
                (uint)VSConstants.VSITEMID.Root,
                (int)__VSHPROPID9.VSHPROPID_IsDeferred,
                out isDeferred)))
            {
                return false;
            }

            return object.Equals(true, isDeferred);
#endif
        }
    }
}
