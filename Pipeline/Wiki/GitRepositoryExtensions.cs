using Nuke.Common.Git;
using Nuke.Common.Tools.GitHub;

namespace Pipeline.Wiki;

public static class GitRepositoryExtensions
{

    public static string GetWikiUrl(this GitRepository repository)
    {
        return $"https://github.com/{repository.GetGitHubOwner()}/{repository.GetGitHubName()}.wiki.git";
    }

    public static GitRepository GetWikiRepository(this GitRepository repository)
    {
        return GitRepository.FromUrl(repository.GetWikiUrl());
    }

    public static string GetAuthenticatedUrl(this GitRepository repository, string username, string token)
    {
        return $"https://{username}:{token}@github.com/{repository.GetGitHubOwner()}/{repository.GetGitHubName()}.git";
    }

    public static object GetDetails(this GitRepository repository)
    {
        var info = new
        {

            repository.Branch,
            repository.Commit,
            repository.Endpoint,
            repository.Head,
            repository.Identifier,
            repository.RemoteBranch,
            repository.RemoteName,
            repository.Tags,
            repository.LocalDirectory,
            repository.Protocol,


            GitHubName = repository.GetGitHubName(),
            GitHubOwner = repository.GetGitHubOwner(),
            DefaultBranch = repository.GetDefaultBranch().Result,


            IsOnDevelopBranch = repository.IsOnDevelopBranch(),
            IsOnFeatureBranch = repository.IsOnFeatureBranch(),
            IsOnHotfixBranch = repository.IsOnHotfixBranch(),
            IsOnMainBranch = repository.IsOnMainBranch(),
            IsOnMainOrMasterBranch = repository.IsOnMainOrMasterBranch(),
            IsOnMasterBranch = repository.IsOnMasterBranch(),
            IsOnReleaseBranch = repository.IsOnReleaseBranch()
        };
        return info;
    }
}