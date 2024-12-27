using Barrel;
using Barrel.Scheduler;
using Xunit.Abstractions;

namespace BarrelTest.Integrations;

/// <summary>
/// Tests related to jobs' delay.
/// We ensure here that jobs are scheduled and run within their delay limitations
/// </summary>
public class DelayTests : IntegrationTest
{
    public DelayTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task JobNoDelayTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        SuccessfulJob job = new SuccessfulJob(CompletionSource);

        Scheduler.Schedule(job);

        await WaitForJobToEnd();
        Assert.Equal(JobState.Success, job.JobState);
    }

    [Fact]
    public async Task JobWithDelayTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        SuccessfulJob job = new SuccessfulJob(CompletionSource);

        Scheduler.Schedule(job, TimeSpan.FromSeconds(1));

        await Task.Delay(100);
        Assert.Equal(JobState.Scheduled, job.JobState);
    }
}
