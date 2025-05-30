// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using NuGet.Frameworks;
using NuGet.ProjectModel;

namespace Microsoft.DotNet.Cli.CommandFactory.CommandResolution;

internal interface IProject
{
    LockFile GetLockFile();

    bool TryGetLockFile(out LockFile lockFile);

    IEnumerable<SingleProjectInfo> GetTools();

    string DepsJsonPath { get; }

    string ProjectRoot { get; }

    string RuntimeConfigJsonPath { get; }

    string FullOutputPath { get; }

    NuGetFramework DotnetCliToolTargetFramework { get; }

    Dictionary<string, string> EnvironmentVariables { get; }

    string ToolDepsJsonGeneratorProject { get; }
}
