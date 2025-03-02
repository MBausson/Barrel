namespace BarrelTest.Integrations;

//  Ensures that jobs' perform methods are executed
public class PerformTests(ITestOutputHelper output) : IntegrationTest(output)
{
    [Fact]
    public async Task PerformAndBeforePerformTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new PerformProofJob();

        Assert.False(job.PerformExecuted);
        Assert.False(job.BeforePerformExecuted);

        Scheduler.Schedule(job);
        await WaitForJobToEnd(job);

        Assert.True(job.PerformExecuted);
        Assert.True(job.BeforePerformExecuted);
    }

    private class PerformProofJob : TestJob
    {
        public bool PerformExecuted { get; private set; }
        public bool BeforePerformExecuted { get; private set; }

        protected override Task BeforePerformAsync()
        {
            BeforePerformExecuted = true;

            return Task.CompletedTask;
        }

        protected override Task PerformAsync()
        {
            PerformExecuted = true;

            _ = Task.Delay(150).ContinueWith(_ => JobFinishedSource.SetResult(true));
            return Task.CompletedTask;
        }
    }
}