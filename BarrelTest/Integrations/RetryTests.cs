﻿namespace BarrelTest.Integrations;

public class RetryTests
{
    public class RetryJobsTests(ITestOutputHelper output) : IntegrationTest(output)
    {
        //  Ensures that a successful job isn't retried
        [Fact]
        public async Task Successful_NoRetryTest()
        {
            Scheduler = new JobScheduler();

            var job = new RetryableJob();
            var jobData = Scheduler.Schedule(job, new ScheduleOptions().WithMaxRetries(3));

            await WaitForJobToEnd(job);

            Assert.Equal(0, jobData.RetryAttempts);
            Assert.Equal(JobState.Success, jobData.JobState);
        }

        //  Ensures that a job that keeps failing is retried as much as permitted
        [Fact]
        public async Task Failure_MaxRetryTest()
        {
            Scheduler = new JobScheduler();

            var job = new RetryableJob(true);
            var jobData = Scheduler.Schedule(job, new ScheduleOptions().WithMaxRetries(3));

            await WaitForJobToEnd(job);

            Assert.Equal(JobState.Failed, jobData.JobState);
            Assert.Equal(3, jobData.RetryAttempts);
        }

        private class RetryableJob(bool shouldFail = false) : TestJob
        {
            private readonly bool _shouldFail = shouldFail;

            protected override async Task PerformAsync()
            {
                if (_shouldFail) throw new Exception("An exception");

                await base.PerformAsync();
            }
        }
    }
}