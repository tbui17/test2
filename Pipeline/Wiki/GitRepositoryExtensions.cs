using Nuke.Common.Git;
using Nuke.Common.Tools.GitHub;

namespace Pipeline.Wiki;

public static class GitRepositoryExtensions
{

    public static string GetWikiHttpsUrl(this GitRepository repository)
    {
        return $"https://github.com/{repository.GetGitHubOwner()}/{repository.GetGitHubName()}.wiki.git";
    }

    public static GitRepository GetWikiRepository(this GitRepository repository)
    {
        return GitRepository.FromUrl(repository.GetWikiHttpsUrl());
    }

    public static string GetAuthenticatedHttpsUrl(this GitRepository repository, AuthenticationDetails authenticationDetails)
    {
        return
            $"https://{authenticationDetails.Username}:{authenticationDetails.Token}@github.com/{repository.GetGitHubOwner()}/{repository.GetGitHubName()}.git";
    }
}

public readonly record struct AuthenticationDetails(string Username, string Token);