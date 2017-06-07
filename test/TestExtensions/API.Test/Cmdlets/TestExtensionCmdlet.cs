// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Management.Automation;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace API.Test.Cmdlets
{
    public abstract class TestExtensionCmdlet : Cmdlet
    {
        protected override void ProcessRecord()
        {
            ThreadHelper.JoinableTaskFactory.Run(() => ProcessRecordAsync());
        }

        protected abstract Task ProcessRecordAsync();
    }
}
