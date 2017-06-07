using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Moq;
using NuGet.Configuration;
using NuGet.VisualStudio;
using Test.Utility.Threading;
using Xunit;

namespace NuGet.SolutionRestoreManager.Test
{
    [Collection(DispatcherThreadCollection.CollectionName)]
    public class SolutionRestoreBuildHandlerTests
    {
        private readonly JoinableTaskFactory _jtf;

        public SolutionRestoreBuildHandlerTests(DispatcherThreadFixture fixture)
        {
            Assumes.Present(fixture);

            _jtf = fixture.JoinableTaskFactory;
        }

        [Fact]
        public async Task QueryDelayBuildAction_CleanBuild()
        {
            var lockService = Mock.Of<INuGetLockService>();
            var settings = Mock.Of<ISettings>();
            var restoreWorker = Mock.Of<ISolutionRestoreWorker>();
            var buildManager = Mock.Of<IVsSolutionBuildManager3>();
            var buildAction = (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_CLEAN;

            using (var handler = new SolutionRestoreBuildHandler(lockService, settings, restoreWorker, buildManager))
            {
                await _jtf.SwitchToMainThreadAsync();

                var result = await handler.RestoreAsync(buildAction, CancellationToken.None);

                Assert.True(result);
            }

            Mock.Get(restoreWorker)
                .Verify(x => x.CleanCache(), Times.Once);
 
            Mock.Get(restoreWorker)
                .Verify(x => x.ScheduleRestoreAsync(It.IsAny<SolutionRestoreRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task QueryDelayBuildAction_ShouldNotRestoreOnBuild_NoOps()
        {
            var lockService = Mock.Of<INuGetLockService>();
            var settings = Mock.Of<ISettings>();
            var restoreWorker = Mock.Of<ISolutionRestoreWorker>();
            var buildManager = Mock.Of<IVsSolutionBuildManager3>();

            var buildAction = (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD;

            Mock.Get(settings)
                .Setup(x => x.GetValue("packageRestore", "automatic", false))
                .Returns(bool.FalseString);

            using (var handler = new SolutionRestoreBuildHandler(lockService, settings, restoreWorker, buildManager))
            {
                await _jtf.SwitchToMainThreadAsync();

                var result = await handler.RestoreAsync(buildAction, CancellationToken.None);

                Assert.True(result);
            }

            Mock.Get(restoreWorker)
                .Verify(x => x.ScheduleRestoreAsync(It.IsAny<SolutionRestoreRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task QueryDelayBuildAction_ShouldNotRestoreOnBuild_ProjectUpToDateMark()
        {
            var lockService = Mock.Of<INuGetLockService>();
            var settings = Mock.Of<ISettings>();
            var restoreWorker = Mock.Of<ISolutionRestoreWorker>();
            var buildManager = Mock.Of<IVsSolutionBuildManager3>();

            var buildAction = (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD + (uint)VSSOLNBUILDUPDATEFLAGS3.SBF_FLAGS_UPTODATE_CHECK;

            Mock.Get(settings)
                .Setup(x => x.GetValue("packageRestore", "automatic", false))
                .Returns(bool.TrueString);

            using (var handler = new SolutionRestoreBuildHandler(lockService, settings, restoreWorker, buildManager))
            {
                await _jtf.SwitchToMainThreadAsync();

                var result = await handler.RestoreAsync(buildAction, CancellationToken.None);

                Assert.True(result);
            }

            Mock.Get(restoreWorker)
                .Verify(x => x.ScheduleRestoreAsync(It.IsAny<SolutionRestoreRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task QueryDelayBuildAction_ShouldRestoreOnBuild()
        {
            var lockService = Mock.Of<INuGetLockService>();
            var settings = Mock.Of<ISettings>();
            var restoreWorker = Mock.Of<ISolutionRestoreWorker>();
            var buildManager = Mock.Of<IVsSolutionBuildManager3>();

            var buildAction = (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD;

            Mock.Get(settings)
                .Setup(x => x.GetValue("packageRestore", "automatic", false))
                .Returns(bool.TrueString);
            Mock.Get(restoreWorker)
                .SetupGet(x => x.JoinableTaskFactory)
                .Returns(_jtf);
            Mock.Get(restoreWorker)
                .Setup(x => x.ScheduleRestoreAsync(It.IsAny<SolutionRestoreRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            using (var handler = new SolutionRestoreBuildHandler(lockService, settings, restoreWorker, buildManager))
            {
                await _jtf.SwitchToMainThreadAsync();

                var result = await handler.RestoreAsync(buildAction, CancellationToken.None);

                Assert.True(result);
            }

            Mock.Get(restoreWorker)
                .Verify(x => x.ScheduleRestoreAsync(It.IsAny<SolutionRestoreRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task QueryDelayBuildAction_LockHeld_BuildDelay()
        {
            var IsRestoreDelayed = false;
            var lockService = Mock.Of<INuGetLockService>();
            var settings = Mock.Of<ISettings>();
            var restoreWorker = Mock.Of<ISolutionRestoreWorker>();
            var buildManager = Mock.Of<IVsSolutionBuildManager3>();

            var buildAction = (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD;

            Mock.Get(lockService)
                .SetupGet(x => x.IsLockHeld)
                .Returns(true);
            Mock.Get(lockService)
                .Setup(x => x.WaitAndReleaseAsync(It.IsAny<CancellationToken>()))
                .Callback((CancellationToken token) =>
                {
                    IsRestoreDelayed = true;
                })
                .Returns(Task.FromResult(true));
            Mock.Get(settings)
                .Setup(x => x.GetValue("packageRestore", "automatic", false))
                .Returns(bool.TrueString);
            Mock.Get(restoreWorker)
                .SetupGet(x => x.JoinableTaskFactory)
                .Returns(_jtf);
            Mock.Get(restoreWorker)
                .Setup(x => x.ScheduleRestoreAsync(It.IsAny<SolutionRestoreRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            using (var handler = new SolutionRestoreBuildHandler(lockService, settings, restoreWorker, buildManager))
            {
                await _jtf.SwitchToMainThreadAsync();

                var result = await handler.RestoreAsync(buildAction, CancellationToken.None);

                Assert.True(result);
                Assert.True(IsRestoreDelayed);
            }

            Mock.Get(restoreWorker)
                .Verify(x => x.ScheduleRestoreAsync(It.IsAny<SolutionRestoreRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
