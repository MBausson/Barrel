using Barrel.Configuration;
using Barrel.Scheduler;
using Xunit.Abstractions;

namespace BarrelTest.Integrations;

/// <summary>
/// Any test class that intends to schedule Jobs should inherit from this class.
/// Even though a test case has finished, its scheduled jobs will remain alive, which could lead to
/// unpredictable behaviours, so they are disposed after each test.
/// <remarks>Please declare the scheduler used in the Scheduler attribute.</remarks>
/// </summary>
public class IntegrationTest : IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected JobSchedulerConfigurationBuilder ConfigurationBuilder = new();
    protected JobScheduler? Scheduler;
    protected readonly int JobWaitTimeout = 5000;

    public IntegrationTest(ITestOutputHelper output)
    {
        ConfigurationBuilder = ConfigurationBuilder
            .WithQueuePollingRate(0)
            .WithSchedulePollingRate(0);

        Output = output;
    }

    protected async Task WaitForJobToRun(TestJob job)
    {
        await Task.WhenAny(job.JobRunningSource.Task, Task.Delay(JobWaitTimeout));
    }

    protected async Task WaitForJobToEnd(TestJob job)
    {
        await Task.WhenAny(job.JobFinishedSource.Task, Task.Delay(JobWaitTimeout));
    }

    public void Dispose()
    {
        if (Scheduler is not null) Scheduler.Dispose();
    }
}
