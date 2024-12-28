using Xunit.Abstractions;

namespace BarrelTest.Integrations;

/// <summary>
///     Tests related to jobs' delay.
///     We ensure here that jobs are scheduled and run within their delay limitations
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
        var job = new SuccessfulJob();

        Scheduler.Schedule(job);
        await WaitForJobToEnd(job);

        Assert.Equal(JobState.Success, job.JobState);
    }

    [Fact]
    public async Task JobWithDelayTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new SuccessfulJob();

        var beforeScheduleTime = DateTime.Now;

        Scheduler.Schedule(job, ScheduleOptions.FromDelay(TimeSpan.FromSeconds(1)));
        await WaitForJobToEnd(job);

        var afterScheduleTime = DateTime.Now;

        Assert.InRange(afterScheduleTime - beforeScheduleTime, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
    }
}