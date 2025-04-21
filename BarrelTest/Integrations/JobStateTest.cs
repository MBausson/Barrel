namespace BarrelTest.Integrations;

/// <summary>
///     Tests related to jobs' status.
/// </summary>
public class JobStateTest : IntegrationTest
{
    public JobStateTest(ITestOutputHelper output) : base(output)
    {
    }

    // Delayed jobs should be Scheduled
    [Fact]
    public void ScheduledStatusTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new SuccessfulJob();

        var jobData = Scheduler.Schedule(job, ScheduleOptions.FromDelay(TimeSpan.FromSeconds(1)));

        Assert.Equal(JobState.Scheduled, jobData.State);
    }

    // Un-delayed jobs should be Enqueued
    [Fact]
    public async Task EnqueuedStatusTest()
    {
        ConfigurationBuilder = ConfigurationBuilder.WithMaxConcurrentJobs(1);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        //  The successfulJob should wait that busyJob finishes
        var busyJob = new BusyJob(2000);
        var successfulJob = new SuccessfulJob();

        Scheduler.Schedule(busyJob);
        var successfulJobData = Scheduler.Schedule(successfulJob);

        await WaitForJobToRun(busyJob);

        Assert.Equal(JobState.Enqueued, successfulJobData.State);
    }

    // Running jobs should be Running
    [Fact]
    public async Task RunningStatusTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new BusyJob(2000);

        var jobData = Scheduler.Schedule(job);

        await WaitForJobToRun(job);

        Assert.Equal(JobState.Running, jobData.State);
    }

    //  Jobs that throw an error should be Failed
    [Fact]
    public async Task FailedStatusTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new FailedJob();

        var jobData = Scheduler.Schedule(job);

        await WaitForJobToEnd(job);

        Assert.Equal(JobState.Failed, jobData.State);
    }

    //  Jobs that run completely should be Success
    [Fact]
    public async Task SuccessStatusTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new SuccessfulJob();

        var jobData = Scheduler.Schedule(job);

        await WaitForJobToEnd(job);

        Assert.Equal(JobState.Success, jobData.State);
    }
}