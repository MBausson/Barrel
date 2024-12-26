using Barrel;
using Barrel.Scheduler;
using Xunit.Abstractions;

namespace BarrelTest.Integrations;

/// <summary>
/// Tests related to the Scheduler's thread pool.
/// We ensure here that we do not run (concurrently) more jobs than we should.
/// When the pool is full of jobs, incoming jobs will wait as Enqueued jobs.
/// </summary>
public class ThreadPoolTests : IntegrationTest
{
    public ThreadPoolTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task EmptyPoolTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new BusyJob(CompletionSource);

        Scheduler.Schedule(job);

        await WaitForJobToEnd();
        Assert.NotEqual(JobState.Enqueued, job.JobState);
    }

    [Fact]
    public async Task FullPoolTest()
    {
        var secondJobCompletionSource = new TaskCompletionSource<bool>();

        ConfigurationBuilder = ConfigurationBuilder.WithMaxThreads(1);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var firstJob = new BusyJob(CompletionSource);
        var secondJob = new BusyJob(secondJobCompletionSource);

        Scheduler.Schedule(firstJob);
        Scheduler.Schedule(secondJob);

        await Task.Delay(300);

        //  The pool is blocked by the first job.
        //  The second job should be waiting enqueued
        Assert.Equal(JobState.Running, firstJob.JobState);
        Assert.Equal(JobState.Enqueued, secondJob.JobState);

        await WaitForJobToEnd();

        //  Now the first job is done and the second one has already started
        Assert.Equal(JobState.Success, firstJob.JobState);
        Assert.Equal(JobState.Running, secondJob.JobState);
    }
}
