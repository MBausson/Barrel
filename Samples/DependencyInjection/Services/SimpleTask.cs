namespace DependencyInjection.Services;

public class SimpleTask : ISimpleTask
{
    public async Task DoWorkAsync()
    {
        Console.WriteLine("-- SimpleTask is doing its work... --");
        await Task.Delay(500);
    }
}
