﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Cosmos.Build.Builder.Dependencies
{
    internal class VisualStudioDependency : IDependency
    {
        private static readonly Version MinimumVsVersion = new Version(15, 7);

        public string Name => $"Visual Studio {MinimumVsVersion.Major}.{MinimumVsVersion.Minor}+";

        private ISetupInstance2 _visualStudioInstance;

        public VisualStudioDependency(ISetupInstance2 visualStudioInstance)
        {
            _visualStudioInstance = visualStudioInstance;
        }

        public Task<bool> IsInstalledAsync(CancellationToken cancellationToken)
        {
            var versionString = _visualStudioInstance.GetInstallationVersion();

            if (Version.TryParse(versionString, out var version)
                && version >= MinimumVsVersion)
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public async Task InstallAsync(CancellationToken cancellationToken)
        {
            var vsInstallerPath = Environment.ExpandEnvironmentVariables(
                @"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vs_installershell.exe");
            var vsInstancePath = _visualStudioInstance.GetInstallationPath();

            var args = $"update --passive --norestart --quiet --installPath \"{vsInstancePath}\"";

            var process = Process.Start(vsInstallerPath, args);
            await Task.Run((Action)process.WaitForExit, cancellationToken).ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                throw new Exception("The process failed to execute!");
            }
        }
    }
}
