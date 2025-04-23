using System.Collections.Generic;
using JetBrains.Annotations;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

namespace Pipeline.Wiki;

public class GitContext(Tool gitTool, [CanBeNull] AbsolutePath workingDirectory = null)
{
    public IReadOnlyCollection<Output> Git(ArgumentStringHandler cmd)
    {
        return gitTool(cmd, workingDirectory);
    }

    public IReadOnlyCollection<Output> Commit(string message)
    {
        return Git($"commit -m \"{message:nq}\"");
    }
}
