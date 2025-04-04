using Barrel;
using DependencyInjection.Services;

namespace DependencyInjection;

public class SimpleJob(ISimpleTask simpleTask) : BaseJob
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
            await simpleTask.DoWorkAsync();
        }

        Console.WriteLine($"Job #{jobName} - Done !");
    }
}
