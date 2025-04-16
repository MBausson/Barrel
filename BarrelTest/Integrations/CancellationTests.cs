using Barrel.Exceptions;
using Barrel.JobData;
using Barrel.JobData.Factory;

namespace BarrelTest.Integrations;

public class CancellationTests(ITestOutputHelper output) : IntegrationTest(output)
{
    [Fact]
    //  Ensures that a job that has a NotStarted status cannot be cancelled
    //  Note: This test is a nice to have. It's not really relevant since this scenario is very unlikely to happen
    public void CancelJobNotStarted_FailsTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var job = new JobDataFactory().Create<ScheduledJobData, SuccessfulJob, ScheduleOptions>(new ScheduleOptions());

        Assert.Throws<ImpossibleJobCancellationException>(() =>
        {
            Scheduler.CancelScheduledJob(job);
        });
    }

    [Fact]
    //  Ensures that a failing job cannot be cancelled
    public async Task CancelJobFailed_FailsTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var job = new FailedJob();
        var jobData = Scheduler.Schedule(job);

        await WaitForJobToEnd(job);

        Assert.Equal(JobState.Failed, jobData.JobState);
        Assert.Throws<ImpossibleJobCancellationException>(() => Scheduler.CancelScheduledJob(jobData));
    }

     [Fact]
     //  Ensures that a successful job cannot be cancelled
     public async Task CancelJobSuccess_FailsTest()
     {
         Scheduler = new JobScheduler(ConfigurationBuilder);

         var job = new SuccessfulJob();
         var jobData = Scheduler.Schedule(job);

         await WaitForJobToEnd(job);

         Assert.Equal(JobState.Success, jobData.JobState);
         Assert.Throws<ImpossibleJobCancellationException>(() => Scheduler.CancelScheduledJob(jobData));
     }
}
