// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Management.Automation;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace API.Test.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Enable, "LightweightSolutionLoad")]
    public sealed class EnableLightweightSolutionLoadCommand : Cmdlet
    {
        private bool _reload;

        [Parameter]
        public SwitchParameter Reload { get => _reload; set => _reload = value; }

        protected override void ProcessRecord()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var vsSolution = ServiceLocator.GetService<SVsSolution, IVsSolution>();
                Assumes.Present(vsSolution);

                ErrorHandler.ThrowOnFailure(vsSolution.SetProperty(
                    (int)__VSPROPID7.VSPROPID_DeferredLoadOption,
                    __VSSOLUTIONDEFERREDLOADOPTION.DLO_DEFERRED));

                if (_reload)
                {
                    var dteSolution = await VSSolutionHelper.GetDTESolutionAsync();
                    Assumes.Present(dteSolution);

                    var solutionFullName = dteSolution.FullName;

                    dteSolution.Close(SaveFirst: true);

                    dteSolution.Open(solutionFullName);
                }
            });
        }
    }
}
