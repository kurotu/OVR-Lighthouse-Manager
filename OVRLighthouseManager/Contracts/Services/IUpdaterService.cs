namespace OVRLighthouseManager.Contracts.Services;
public interface IUpdaterService
{
    public bool HasUpdate
    {
        get;
    }
    public string? LatestVersion
    {
        get;
    }

    public event EventHandler<string> FoundUpdate;

    public Task FetchLatestVersion();
}
