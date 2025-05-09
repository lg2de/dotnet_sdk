// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.DotNet.Cli.ToolPackage;

internal interface ILocalToolsResolverCache
{
    void Save(
        IDictionary<RestoredCommandIdentifier, RestoredCommand> restoredCommandMap);

    bool TryLoad(
        RestoredCommandIdentifier restoredCommandIdentifier,
        out RestoredCommand restoredCommand);

    bool TryLoadHighestVersion(
        RestoredCommandIdentifierVersionRange query,
        out RestoredCommand restoredCommandList);
}
