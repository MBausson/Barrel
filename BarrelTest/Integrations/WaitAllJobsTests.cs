namespace BarrelTest.Integrations;

public class WaitAllJobsTests(ITestOutputHelper output) : IntegrationTest(output)
{
    [Fact]
    //  If no jobs have been scheduled, we shouldn't be waiting too long
    public async Task WithNoJobs()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var beforeTime = DateTimeOffset.UtcNow;

        await Scheduler.WaitAllJobsAsync();

        var afterTime = DateTimeOffset.UtcNow;
        var duration = afterTime - beforeTime;

        Assert.InRange(duration, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    [Fact]
    //  If a 1-second job has been scheduled, we should wait at least 1 second
    public async Task WithOneRunningJob()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new BusyJob(1000);
        var beforeTime = DateTimeOffset.UtcNow;

        var jobData = Scheduler.Schedule(job);
        await Scheduler.WaitAllJobsAsync();

        var afterTime = DateTimeOffset.UtcNow;
        var duration = afterTime - beforeTime;

        Assert.InRange(duration, TimeSpan.FromSeconds(1), TimeSpan.MaxValue);
        Assert.Equal(JobState.Success, jobData.State);
    }

    [Fact]
    //  1-second job delayed by 1 second
    //  1-second job with no delay
    //  We should overall wait for at least 2 seconds
    //  This test ensures that we also take into account schedules, and not only running jobs
    public async Task WithMultipleEnqueuedJobs()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var noDelayJob = new BusyJob(1000);
        var delayedJob = new BusyJob(1000);
        var beforeTime = DateTimeOffset.UtcNow;

        var noDelayJobData = Scheduler.Schedule(noDelayJob);
        var delayedJobData = Scheduler.Schedule(delayedJob, ScheduleOptions.FromDelay(TimeSpan.FromSeconds(1)));

        await Scheduler.WaitAllJobsAsync();

        var afterTime = DateTimeOffset.UtcNow;
        var duration = afterTime - beforeTime;

        Assert.InRange(duration, TimeSpan.FromSeconds(2), TimeSpan.MaxValue);
        Assert.Equal(JobState.Success, noDelayJobData.State);
        Assert.Equal(JobState.Success, delayedJobData.State);
    }
}