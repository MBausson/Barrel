using Barrel;
using Barrel.Configuration;
using Barrel.Scheduler;

namespace BarrelTest.Integrations;

/// <summary>
/// Tests related to jobs' delay.
/// We ensure here that jobs are scheduled and run within their delay limitations
/// </summary>
public class DelayTests : IntegrationTest
{
    [Fact]
    public async Task JobNoDelayTest()
    {
        var scheduler = new JobScheduler(ConfigurationBuilder);
        SimpleJob jobNoDelay = new SimpleJob();

        scheduler.Schedule(jobNoDelay);

        await Task.Delay(50);
        Assert.Equal(JobState.Success, jobNoDelay.JobState);
    }

    [Fact]
    public async Task JobWithDelayTest()
    {
        var scheduler = new JobScheduler(ConfigurationBuilder);
        SimpleJob job1SecondDelay = new SimpleJob();

        scheduler.Schedule(job1SecondDelay, TimeSpan.FromSeconds(1));

        await Task.Delay(50);
        Assert.Equal(JobState.Scheduled, job1SecondDelay.JobState);
    }
}
