using Barrel;
using Barrel.Scheduler;

namespace BarrelTest.Integrations;

/// <summary>
/// Tests related to the scheduler's thread pool.
/// We ensure here that we do not run (concurrently) more jobs than we should.
/// When the pool is full of jobs, incoming jobs will wait as Enqueued jobs.
/// </summary>
public class ThreadPoolTests : IntegrationTest
{
    [Fact]
    public async Task EmptyPoolTest()
    {
        var scheduler = new JobScheduler(ConfigurationBuilder);
        var job = new BusyJob();

        scheduler.Schedule(job);

        await Task.Delay(100);
        Assert.NotEqual(JobState.Enqueued, job.JobState);
    }

    [Fact]
    public async Task FullPoolTest()
    {
        ConfigurationBuilder = ConfigurationBuilder.WithMaxThreads(1);
        var scheduler = new JobScheduler(ConfigurationBuilder);

        var firstJob = new BusyJob();
        var secondJob = new BusyJob();

        scheduler.Schedule(firstJob);
        scheduler.Schedule(secondJob);

        await Task.Delay(100);

        //  The pool is blocked by the first job.
        //  The second job should be waiting enqueued
        Assert.Equal(JobState.Running, firstJob.JobState);
        Assert.Equal(JobState.Enqueued, secondJob.JobState);

        await Task.Delay(500);

        //  Now the first job is done and the second one has already started
        Assert.Equal(JobState.Success, firstJob.JobState);
        Assert.Equal(JobState.Running, secondJob.JobState);
    }
}
