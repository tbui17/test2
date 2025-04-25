using JetBrains.Annotations;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

namespace Pipeline.Wiki;

public class GitContextFactory(Tool gitTool)
{
    public GitContext Create([CanBeNull] AbsolutePath workingDirectory = null)
    {
        workingDirectory ??= AbsolutePath.Create(".");
        return new GitContext(gitTool, workingDirectory);
    }
}
