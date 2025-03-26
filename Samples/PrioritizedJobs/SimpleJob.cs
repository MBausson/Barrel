using Barrel;

namespace PrioritizedJobs;

public class SimpleJob(string prefix) : BaseJob
{
    private static int _jobCount;

    protected override Task BeforePerformAsync()
    {
        _jobCount++;

        return Task.CompletedTask;
    }

    protected override async Task PerformAsync()
    {
        var jobName = _jobCount;

        Console.WriteLine($"[{prefix}] Job #{jobName} - Starting");

        for (var k = 0; k < 10; k++)
        {
            Console.WriteLine($"[{prefix}] Job #{jobName} -- {k / 0.1f} %");
            await Task.Delay(500);
        }

        Console.WriteLine($"[{prefix}] Job #{jobName} - Done !");
    }
}
