using Barrel.Scheduler;
using Xunit.Abstractions;

namespace BarrelTest.Integrations;

//  Ensures that jobs' perfom method are executed
public class PerformTests(ITestOutputHelper output) : IntegrationTest(output)
{
    [Fact]
    public async Task PerformAndBeforePerformTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new PerformProofJob(CompletionSource);

        Assert.False(job.PerformExecuted);
        Assert.False(job.BeforePerformExecuted);

        Scheduler.Schedule(job);
        await WaitForJobToEnd();

        Assert.True(job.PerformExecuted);
        Assert.True(job.BeforePerformExecuted);
    }

    private class PerformProofJob(TaskCompletionSource<bool> completionSource) : TestJob(completionSource)
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

            _ = Task.Delay(150).ContinueWith(_ => CompletionSource.SetResult(true));
            return Task.CompletedTask;
        }
    }
}
