using Octokit;
using OVRLighthouseManager.Contracts.Services;
using OVRLighthouseManager.Helpers;
using Semver;
using Serilog;

namespace OVRLighthouseManager.Services;
internal class UpdaterService : IUpdaterService
{
    public string? LatestVersion => _latestVersion?.ToString();

    public bool HasUpdate => _latestVersion != null && _latestVersion.ComparePrecedenceTo(_currentVersion) > 0;
    public event EventHandler<string> FoundUpdate = delegate { };

    private static readonly GitHubClient _github = new(new ProductHeaderValue("OVR-Lighthouse-Manager"));

    private readonly SemVersion _currentVersion = SemVersion.Parse(VersionHelper.GetVersion(), SemVersionStyles.AllowV);
    private bool _allowPrerelease => _currentVersion.IsPrerelease;
    private SemVersion? _latestVersion;

    private readonly ILogger _log = LogHelper.ForContext<UpdaterService>();

    public async Task FetchLatestVersion()
    {
        try
        {
            _latestVersion = await GetLatestVersionAsync(_allowPrerelease);
            if (_latestVersion == null)
            {
                return;
            }
            _log.Debug("Latest version is {LatestVersion}", _latestVersion);
            if (HasUpdate)
            {
                _log.Information("Found update {LatestVersion}", _latestVersion);
                FoundUpdate(this, _latestVersion.ToString());
            }
            else
            {
                _log.Debug("No update found");
            }
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to fetch latest version");
        }
    }

    private static async Task<SemVersion?> GetLatestVersionAsync(bool includePrerelease)
    {
        var releases = await _github.Repository.Release.GetAll("kurotu", "OVR-Lighthouse-Manager");
        var allReleases = releases.Where(r => r.Prerelease == includePrerelease);
        var allVersions = allReleases.Select(r =>
        {
            var result = SemVersion.TryParse(r.TagName, SemVersionStyles.AllowV, out var version);
            return result ? version : null;
        }).Where(v => v != null).ToList();
        return allVersions.Max();
    }
}
