using System.Collections.Generic;
using JetBrains.Annotations;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

namespace Pipeline.Wiki;

public class GitContext(Tool gitTool,[CanBeNull] AbsolutePath workingDirectory)
{

    public IReadOnlyCollection<Output> Git(ArgumentStringHandler cmd)
    {
        return gitTool(cmd, workingDirectory);
    }
}