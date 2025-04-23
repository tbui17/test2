using JetBrains.Annotations;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

namespace Pipeline.Wiki;

public class GitContextFactory(Tool gitTool)
{
    public GitContext Create([CanBeNull] AbsolutePath workingDirectory = null)
    {
        return new GitContext(gitTool, workingDirectory);
    }
}
