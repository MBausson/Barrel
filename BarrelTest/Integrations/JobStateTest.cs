using Barrel;
using Barrel.Scheduler;
using Xunit.Abstractions;

namespace BarrelTest.Integrations;

/// <summary>
/// Tests related to jobs' status.
/// </summary>
public class JobStateTest : IntegrationTest
{
    public JobStateTest(ITestOutputHelper output) : base(output)
    {
    }

    // Un-scheduled jobs should be NotStarted
    [Fact]
    public void NotStartedStatusTest()
    {
        var job = new SuccessfulJob();

        Assert.Equal(JobState.NotStarted, job.JobState);
    }

    // Delayed jobs should be Scheduled
    [Fact]
    public void ScheduledStatusTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new SuccessfulJob();

        Scheduler.Schedule(job, TimeSpan.FromSeconds(1));

        Assert.Equal(JobState.Scheduled, job.JobState);
    }

    // Un-delayed jobs should be Enqueued
    [Fact]
    public async Task EnqueuedStatusTest()
    {
        ConfigurationBuilder = ConfigurationBuilder.WithMaxThreads(1);

        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new SuccessfulJob();

        Scheduler.Schedule<BusyJob>();
        Scheduler.Schedule(job);

        await Task.Delay(300);
        Assert.Equal(JobState.Enqueued, job.JobState);
    }

    // Running jobs should be Running
    [Fact]
    public async Task RunningStatusTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new BusyJob(jobDurationMilliseconds: 2000);

        Scheduler.Schedule(job);

        await Task.Delay(300);
        Assert.Equal(JobState.Running, job.JobState);
    }

    //  Jobs that throw an error should be Failed
    [Fact]
    public async Task FailedStatusTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new FailedJob();

        Scheduler.Schedule(job);

        await Task.Delay(300);
        Assert.Equal(JobState.Failed, job.JobState);
    }

    //  Jobs that run completely should be Success
    [Fact]
    public async Task SuccessStatusTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new SuccessfulJob();

        Scheduler.Schedule(job);

        await Task.Delay(300);
        Assert.Equal(JobState.Success, job.JobState);
    }
}
