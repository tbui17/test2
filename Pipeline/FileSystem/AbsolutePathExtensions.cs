using System.Collections.Generic;
using System.Linq;
using Nuke.Common.IO;

namespace Pipeline.FileSystem;

public static class AbsolutePathExtensions
{
    public static IEnumerable<AbsolutePath> Children(this AbsolutePath path)
    {
        return path.GetDirectories().Concat(path.GetFiles());
    }

    public static AbsolutePath CopyContents(this AbsolutePath path, AbsolutePath target)
    {
        foreach (var c in path.Children())
        {
            c.CopyToDirectory(target, ExistsPolicy.MergeAndOverwrite);
        }

        return path;
    }


}
