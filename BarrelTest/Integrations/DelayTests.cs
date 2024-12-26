using Barrel;
using Barrel.Configuration;
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
        SuccessfulJob jobNoDelay = new SuccessfulJob();

        Scheduler.Schedule(jobNoDelay);

        await Task.Delay(50);
        Assert.Equal(JobState.Success, jobNoDelay.JobState);
    }

    [Fact]
    public async Task JobWithDelayTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        SuccessfulJob job1SecondDelay = new SuccessfulJob();

        Scheduler.Schedule(job1SecondDelay, TimeSpan.FromSeconds(1));

        await Task.Delay(50);
        Assert.Equal(JobState.Scheduled, job1SecondDelay.JobState);
    }
}
