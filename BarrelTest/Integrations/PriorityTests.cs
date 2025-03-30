namespace BarrelTest.Integrations;

public class PriorityTests(ITestOutputHelper output) : IntegrationTest(output)
{
    //  Ensures that jobs with the same priority get executed sequentially (first come first served)
    [Fact]
    public async Task SamePriority_SequentialExecutionTest()
    {
        var priority = (JobPriority)Random.Shared.NextInt64(1, 4);

        ConfigurationBuilder.WithMaxConcurrentJobs(1);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        //  Execute 3 jobs sharing the same random priority
        //  The first job should be executed first, the third job should be executed last

        PriorityJob[] jobsInstances = [new(), new(), new()];

        foreach (var job in jobsInstances)
            Scheduler.Schedule(job, ScheduleOptions.FromPriority(priority));

        await Scheduler.WaitAllJobs();

        Assert.All(
            jobsInstances.SkipLast(1),
            (job, i) => Assert.True(job.ExecutedOn < jobsInstances[i + 1].ExecutedOn));
    }

    //  Ensures that a high job is executed before a medium job
    [Fact]
    public async Task HighPriority_ExecutedBeforeMedium()
    {
        ConfigurationBuilder.WithMaxConcurrentJobs(1);
        //  Set the queue polling rate to default. Since it is set to 0 in IntegrationTest.cs
        //      the test can not work in this condition
        ConfigurationBuilder.WithQueuePollingRate(100);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        //  Schedule one medium job, then a medium job and lastly a high job
        //  The first medium job should be executed first, then the high job should be executed

        var firstMediumJob = new PriorityJob();
        var secondMediumJob = new PriorityJob();
        var highJob = new PriorityJob();

        Scheduler.Schedule(firstMediumJob, ScheduleOptions.FromPriority(JobPriority.Medium));

        //  Since we schedule these jobs so fast, the queue might start directly with the High job
        //  We introduce this delay to make sure the first Medium job gets executed first
        await Task.Delay(120);

        Scheduler.Schedule(secondMediumJob, ScheduleOptions.FromPriority(JobPriority.Medium));
        Scheduler.Schedule(highJob, ScheduleOptions.FromPriority(JobPriority.High));

        await Scheduler.WaitAllJobs();

        //  Asserts that highJob has been executed before secondMediumJob
        Assert.True(highJob.ExecutedOn < secondMediumJob.ExecutedOn);
    }

    //  Ensures that a medium job is executed before a low job
    [Fact]
    public async Task MediumPriority_ExecutedBeforeLow()
    {
        ConfigurationBuilder.WithMaxConcurrentJobs(1);
        //  Set the queue polling rate to default. Since it is set to 0 in IntegrationTest.cs
        //      the test can not work in this condition
        ConfigurationBuilder.WithQueuePollingRate(100);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        //  Schedule one medium job, then a low job and lastly a medium job
        //  The first medium job should be executed first, then the second medium job should be executed
        var firstMediumJob = new PriorityJob();
        var lowJob = new PriorityJob();
        var secondMediumJob = new PriorityJob();

        Scheduler.Schedule(firstMediumJob, ScheduleOptions.FromPriority(JobPriority.Medium));

        //  Since we schedule these jobs so fast, the queue might start directly with the High job
        //  We introduce this delay to make sure the first Medium job gets executed first
        await Task.Delay(120);

        Scheduler.Schedule(lowJob, ScheduleOptions.FromPriority(JobPriority.Low));
        Scheduler.Schedule(secondMediumJob, ScheduleOptions.FromPriority(JobPriority.Medium));

        await Scheduler.WaitAllJobs();

        //  Asserts that secondMediumJob has been executed before lowJob
        Assert.True(secondMediumJob.ExecutedOn < lowJob.ExecutedOn);
    }

    //  Ensures that a high job is executed before a low job
    [Fact]
    public async Task HighPriority_ExecutedBeforeLow()
    {
        ConfigurationBuilder.WithMaxConcurrentJobs(1);
        //  Set the queue polling rate to default. Since it is set to 0 in IntegrationTest.cs
        //      the test can not work in this condition
        ConfigurationBuilder.WithQueuePollingRate(100);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        //  Schedule one medium job, then a low job and lastly a high job
        //  The first medium job should be executed first, then the high job should be executed
        var firstMediumJob = new PriorityJob();
        var lowJob = new PriorityJob();
        var highJob = new PriorityJob();

        Scheduler.Schedule(firstMediumJob, ScheduleOptions.FromPriority(JobPriority.Medium));

        //  Since we schedule these jobs so fast, the queue might start directly with the High job
        //  We introduce this delay to make sure the first Medium job gets executed first
        await Task.Delay(120);

        Scheduler.Schedule(lowJob, ScheduleOptions.FromPriority(JobPriority.Low));
        Scheduler.Schedule(highJob, ScheduleOptions.FromPriority(JobPriority.High));

        await Scheduler.WaitAllJobs();

        //  Asserts that secondMediumJob has been executed before lowJob
        Assert.True(highJob.ExecutedOn < lowJob.ExecutedOn);
    }

    private class PriorityJob : BaseJob
    {
        public DateTime ExecutedOn { get; private set; }

        protected override async Task PerformAsync()
        {
            ExecutedOn = DateTime.Now;

            await Task.Delay(200);
        }
    }
}