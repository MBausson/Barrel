using Barrel;

namespace BarrelTest;

public class BusyJob(int jobDurationMilliseconds) : BaseJob
{
    private readonly int _jobDurationMilliseconds = jobDurationMilliseconds;

    public BusyJob() : this(600)
    {
    }

    protected override async Task PerformAsync()
    {
        await Task.Delay(_jobDurationMilliseconds);
    }
}
