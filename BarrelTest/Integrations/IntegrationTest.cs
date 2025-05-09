﻿using Barrel.JobData;

namespace BarrelTest.Integrations;

/// <summary>
///     Any test class that intends to schedule Jobs should inherit from this class.
///     Even though a test case has finished, its scheduled jobs will remain alive, which could lead to
///     unpredictable behaviours, so they are disposed after each test.
///     <remarks>Please declare the scheduler used in the Scheduler attribute.</remarks>
/// </summary>
public class IntegrationTest : XunitContextBase, IDisposable
{
    protected readonly int JobWaitTimeout = 5000;
    protected new readonly ITestOutputHelper Output;
    protected JobSchedulerConfigurationBuilder ConfigurationBuilder = new();
    protected JobScheduler? Scheduler;

    public IntegrationTest(ITestOutputHelper output) : base(output)
    {
        ConfigurationBuilder = ConfigurationBuilder
            .WithQueuePollingRate(20)
            .WithSchedulePollingRate(20);

        Output = output;
    }

    public new void Dispose()
    {
        if (Scheduler is not null) Scheduler.Dispose();
    }

    protected async Task WaitForJobToRun(TestJob job)
    {
        await Task.WhenAny(job.JobRunningSource.Task, Task.Delay(JobWaitTimeout));
    }

    protected async Task WaitForNonInstancedJobToRun(BaseJobData jobData)
    {
        await Task.WhenAny(Task.Run(async () =>
        {
            while (jobData.State != JobState.Running) await Task.Delay(5);
        }), Task.Delay(JobWaitTimeout));
    }

    protected async Task WaitForJobToEnd(TestJob job)
    {
        await Task.WhenAny(job.JobFinishedSource.Task, Task.Delay(JobWaitTimeout));
    }
}