using System.Reflection.PortableExecutable;
using Barrel.Exceptions;
using Barrel.JobData;
using Barrel.JobData.Factory;

namespace BarrelTest.Integrations;

public class JobOperationsTests(ITestOutputHelper output) : IntegrationTest(output)
{
    [Fact]
    //  Ensures that a job that has a NotStarted status cannot be cancelled
    //  Note: This test is a nice to have. It's not really relevant since this scenario is very unlikely to happen
    public void CancelJobNotStarted_FailsTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var job = new JobDataFactory().Create<ScheduledJobData, SuccessfulJob, ScheduleOptions>(new ScheduleOptions());

        Assert.Throws<JobOperationNotPermitted>(() => { Scheduler.CancelJob(job); });
    }

    [Fact]
    //  Ensures that a failing job cannot be cancelled
    public async Task CancelJobFailed_FailsTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var job = new FailedJob();
        var jobData = Scheduler.Schedule(job);

        await WaitForJobToEnd(job);

        Assert.Equal(JobState.Failed, jobData.State);
        Assert.Throws<JobOperationNotPermitted>(() => Scheduler.CancelJob(jobData));
    }

    [Fact]
    //  Ensures that a successful job cannot be cancelled
    public async Task CancelJobSuccess_FailsTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var job = new SuccessfulJob();
        var jobData = Scheduler.Schedule(job);

        await WaitForJobToEnd(job);

        Assert.Equal(JobState.Success, jobData.State);
        Assert.Throws<JobOperationNotPermitted>(() => Scheduler.CancelJob(jobData));
    }

    [Fact]
    // Ensures that a scheduled job is effectively cancelled when requested
    public void CancelScheduledJob_SucceedsTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var jobData = Scheduler.Schedule<SuccessfulJob>(ScheduleOptions.FromDelay(TimeSpan.FromSeconds(3)));

        Assert.Equal(JobState.Scheduled, jobData.State);
        Scheduler.CancelJob(jobData);
        Assert.Equal(JobState.Cancelled, jobData.State);
    }

    [Fact]
    // Ensures that an enqueued job is effectively cancelled when requested
    public async Task CancelEnqueuedJob_SucceedsTest()
    {
        ConfigurationBuilder.WithMaxConcurrentJobs(1);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var busyJob = new BusyJob(5000);
        Scheduler.Schedule(busyJob);

        await WaitForJobToRun(busyJob);

        var jobToCancelData = Scheduler.Schedule<SuccessfulJob>();

        await Task.Delay(50);

        Assert.Equal(JobState.Enqueued, jobToCancelData.State);
        Scheduler.CancelJob(jobToCancelData);
        Assert.Equal(JobState.Cancelled, jobToCancelData.State);
    }

    [Fact]
    public void PerformNowJobNotStarted_FailsTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var job = new JobDataFactory().Create<ScheduledJobData, SuccessfulJob, ScheduleOptions>(new ScheduleOptions());

        Assert.Throws<JobOperationNotPermitted>(() => { Scheduler.PerformNow(job); });
    }

    [Fact]
    //  Ensures that a failing job cannot be cancelled
    public async Task PerformNowJobFailed_FailsTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var job = new FailedJob();
        var jobData = Scheduler.Schedule(job);

        await WaitForJobToEnd(job);

        Assert.Equal(JobState.Failed, jobData.State);
        Assert.Throws<JobOperationNotPermitted>(() => Scheduler.PerformNow(jobData));
    }

    [Fact]
    //  Ensures that a successful job cannot be performed now
    public async Task PerformNowJobSuccess_FailsTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var job = new SuccessfulJob();
        var jobData = Scheduler.Schedule(job);

        await WaitForJobToEnd(job);

        Assert.Equal(JobState.Success, jobData.State);
        Assert.Throws<JobOperationNotPermitted>(() => Scheduler.PerformNow(jobData));
    }

    [Fact]
    // Ensures that a scheduled job is effectively performed now when requested
    public async Task PerformNowScheduledJob_SucceedsTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var jobData = Scheduler.Schedule<SuccessfulJob>(ScheduleOptions.FromDelay(TimeSpan.FromSeconds(40)));

        Assert.Equal(JobState.Scheduled, jobData.State);
        Scheduler.PerformNow(jobData);

        await Scheduler.WaitAllJobsAsync();
        Assert.Equal(JobState.Success, jobData.State);
    }
}
