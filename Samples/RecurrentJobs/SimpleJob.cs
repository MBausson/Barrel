using Barrel;

namespace RecurrentJobs;

public class SimpleJob : BaseJob
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

        Console.WriteLine($"Job #{jobName} - Starting");

        for (var k = 0; k < 10; k++)
        {
            Console.WriteLine($"Job #{jobName} -- {k / 0.1f} %");
            await Task.Delay(500);
        }

        Console.WriteLine($"Job #{jobName} - Done !");
    }
}