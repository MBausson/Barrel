﻿namespace BarrelTest.Integrations;

/// <summary>
///     Tests related to the Scheduler's job pool.
///     We ensure here that we do not run (concurrently) more jobs than we should.
///     When the pool is full of jobs, incoming jobs will wait as Enqueued jobs.
/// </summary>
public class JobPoolTests : IntegrationTest
{
    public JobPoolTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task EmptyPoolTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new BusyJob();

        var jobData = Scheduler.Schedule(job);
        await WaitForJobToEnd(job);

        Assert.NotEqual(JobState.Enqueued, jobData.State);
    }

    //  This test really doesn't want to pass in CI, it's a flaky test
    //  To fix it, we should implement some kind of TaskCompletionSource but for the different states of a job
    //  Scheduled, Enqueued, Running(!)...
#if !CI
    [Fact]
    public async Task FullPoolTest()
    {
        ConfigurationBuilder = ConfigurationBuilder.WithMaxConcurrentJobs(1);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var firstJob = new BusyJob(2000);
        var secondJob = new BusyJob();

        var firstJobData = Scheduler.Schedule(firstJob);
        var secondJobData = Scheduler.Schedule(secondJob);

        await WaitForJobToRun(firstJob);

        //  The pool is blocked by the first job.
        //  The second job should be waiting enqueued
        Assert.Equal(JobState.Running, firstJobData.State);
        Assert.Equal(JobState.Enqueued, secondJobData.State);

        await WaitForJobToEnd(firstJob);

        //  Now the first job is done and the second one has already started
        Assert.Equal(JobState.Success, firstJobData.State);
        Assert.Equal(JobState.Running, secondJobData.State);
    }
#endif
}