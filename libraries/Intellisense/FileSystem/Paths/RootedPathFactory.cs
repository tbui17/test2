﻿namespace Intellisense.FileSystem.Paths;

internal interface IRootedPathFactory
{
    IFileSystemPath Create(string path);
}

internal class RootedPathFactory : IRootedPathFactory
{
    public IFileSystemPath Create(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            return EmptyPath.Instance;
        }

        // we only care about windows or unix
        if (OperatingSystem.IsWindows())
        {
            return new WindowsRootedPath(path);
        }

        return new UnixRootedPath(path);
    }

    public T CreateOrThrow<T>(string path) where T : IFileSystemPath
    {
        var res = Create(path);
        if (res is not T rootedPath)
        {
            throw new ArgumentException($"Failed to create {nameof(T)} from {path}. OS: {Environment.OSVersion}");
        }

        return rootedPath;
    }
}
