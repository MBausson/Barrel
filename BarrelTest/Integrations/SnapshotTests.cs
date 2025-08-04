namespace BarrelTest.Integrations;

public class SnapshotTests(ITestOutputHelper output) : IntegrationTest(output)
{
    [Fact]
    public void ScheduledJobsTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        Scheduler.Schedule<SuccessfulJob>(ScheduleOptions.FromDelay(TimeSpan.FromSeconds(5)));

        var scheduledJobs = Scheduler.TakeSnapshot().ScheduledJobs.ToArray();
        Assert.Single(scheduledJobs);

        var scheduledJob = scheduledJobs.First();
        Assert.Equal(JobState.Scheduled, scheduledJob.State);
        Assert.Equal(typeof(SuccessfulJob), scheduledJob.JobClass);
    }

    [Fact]
    public async Task WaitingJobsTest()
    {
        ConfigurationBuilder.WithMaxConcurrentJobs(1);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var busyJob = new BusyJob(5000);

        Scheduler.Schedule(busyJob);
        Scheduler.Schedule<SuccessfulJob>();

        await WaitForJobToRun(busyJob);

        //  Flaky test fix :/
        await Task.Delay(150);

        var waitingJobs = Scheduler.TakeSnapshot().WaitingJobs.ToArray();
        Assert.Single(waitingJobs);

        var waitingJob = waitingJobs.First();
        Assert.Equal(JobState.Enqueued, waitingJob.State);
        Assert.Equal(typeof(SuccessfulJob), waitingJob.JobClass);
    }

    [Fact]
    public async Task RunningJobsTest()
    {
        ConfigurationBuilder.WithMaxConcurrentJobs(1);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var busyJob = new BusyJob(5000);
        Scheduler.Schedule(busyJob);

        await WaitForJobToRun(busyJob);

        var runningJobs = Scheduler.TakeSnapshot().RunningJobs.ToArray();
        Assert.Single(runningJobs);

        var runningJob = runningJobs.First();
        Assert.Equal(JobState.Running, runningJob.State);
        Assert.Equal(typeof(BusyJob), runningJob.JobClass);
    }
}
