namespace BarrelTest;

public class SuccessfulJob : TestJob
{
    protected override async Task PerformAsync()
    {
        await base.PerformAsync();
    }
}
