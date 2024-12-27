namespace BarrelTest;

public class FailedJob : TestJob
{
    protected override async Task PerformAsync()
    {
        await base.PerformAsync();

        throw new Exception("Predictable job exception");
    }
}
