namespace BarrelTest.Integrations;

/// <summary>
///     Ensures that "non instanced" jobs are instanced before running.
/// </summary>
public class JobInstanceTests : IntegrationTest
{
    public JobInstanceTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task NonInstancedJob_Test()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var jobData = Scheduler.Schedule<WitnessJob>(ScheduleOptions.FromDelay(TimeSpan.FromSeconds(1)));
        Assert.False(WitnessJob.HasInstance);

        await WaitForNonInstancedJobToRun(jobData);

        Assert.True(WitnessJob.HasInstance);
    }

    [Fact]
    public async Task InstancedJob_Test()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var jobData = Scheduler.Schedule(new WitnessJob(), ScheduleOptions.FromDelay(TimeSpan.FromSeconds(1)));
        Assert.True(WitnessJob.HasInstance);

        await WaitForNonInstancedJobToRun(jobData);

        Assert.True(WitnessJob.HasInstance);
    }

    private class WitnessJob : TestJob
    {
        public WitnessJob()
        {
            HasInstance = true;
        }

        public static bool HasInstance { get; private set; }

        protected override async Task PerformAsync()
        {
            await Task.Delay(1000);
            await base.PerformAsync();
        }
    }
}