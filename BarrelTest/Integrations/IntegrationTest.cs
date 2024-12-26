using Barrel.Configuration;

namespace BarrelTest.Integrations;

public class IntegrationTest
{
    protected JobSchedulerConfigurationBuilder ConfigurationBuilder = new();

    public IntegrationTest()
    {
        ConfigurationBuilder = ConfigurationBuilder
            .WithQueuePollingRate(0)
            .WithSchedulePollingRate(0);
    }
}
