namespace BarrelTest.Integrations;

public class PriorityTests(ITestOutputHelper output) : IntegrationTest(output)
{
    //  Ensures that jobs with different priorities get executed sequentially (first come first served)
    //      when the job queue is not full (MaxConcurrentJobs is not reached)
    [Fact]
    public async Task DifferentPriority_EmptyJobQueue_SequentialExecutionTest()
    {
        ConfigurationBuilder.WithMaxConcurrentJobs(1000);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        //  Execute 3 jobs: first low, second medium, third high priority
        //  The first job should be executed first, the third job should be executed last

        PriorityJob[] jobsInstances = [new(), new(), new()];

        for (var index = 0; index < jobsInstances.Length; index++)
        {
            var job = jobsInstances[index];
            //  The +1 refers to the "Default" priority being at index 0
            var priority = (JobPriority)index + 1;

            Scheduler.Schedule(job, ScheduleOptions.FromPriority(priority));
        }

        await Scheduler.WaitAllJobs();

        Assert.All(
            jobsInstances.SkipLast(1),
            (job, i) => Assert.True(job.ExecutedOn < jobsInstances[i + 1].ExecutedOn));
    }

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
    // [Fact]

    private class PriorityJob : BaseJob
    {
        public DateTime ExecutedOn { get; private set; }

        protected override Task PerformAsync()
        {
            ExecutedOn = DateTime.Now;

            return Task.CompletedTask;
        }
    }
}
