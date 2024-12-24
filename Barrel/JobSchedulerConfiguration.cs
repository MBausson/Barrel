namespace Barrel;

public struct JobSchedulerConfiguration()
{
    public int MaxThreads { get; private set; } = 5;

    public JobSchedulerConfiguration WithMaxThreads(int maxThreads)
    {
        if (maxThreads < 1) throw new ArgumentException("MaxThread property must be greater than zero");

        MaxThreads = maxThreads;

        return this;
    }
}
