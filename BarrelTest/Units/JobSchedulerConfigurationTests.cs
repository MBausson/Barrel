using Microsoft.Extensions.Logging.Abstractions;

namespace BarrelTest.Units;

public class JobSchedulerConfigurationTests
{
    private readonly JobSchedulerConfigurationBuilder _builder = new();
    private JobSchedulerConfiguration Configuration => _builder.Build();

    [Fact]
    public void WithMaxConcurrentJobs_NegativeInputTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _builder.WithMaxConcurrentJobs(Random.Shared.Next(int.MinValue, 0));
        });
    }

    [Fact]
    public void WithMaxConcurrentJobs_ZeroInputTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => { _builder.WithMaxConcurrentJobs(0); });
    }

    [Fact]
    public void WithMaxConcurrentJobs_SetsMaxConcurrentJobsTest()
    {
        var value = Random.Shared.Next(1, int.MaxValue);

        _builder.WithMaxConcurrentJobs(value);

        Assert.Equal(value, Configuration.MaxConcurrentJobs);
    }

    [Fact]
    public void WithQueuePollingRate_NegativeInputTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _builder.WithQueuePollingRate(Random.Shared.Next(int.MinValue, 0));
        });
    }

    [Fact]
    public void WithQueuePollingRate_SetsQueuePollingRateTest()
    {
        var value = Random.Shared.Next(0, int.MaxValue);

        _builder.WithQueuePollingRate(value);

        Assert.Equal(value, Configuration.QueuePollingRate);
    }

    [Fact]
    public void WithSchedulePollingRate_NegativeInputTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _builder.WithSchedulePollingRate(Random.Shared.Next(int.MinValue, 0));
        });
    }

    [Fact]
    public void WithSchedulePollingRate_SetsQueuePollingRateTest()
    {
        var value = Random.Shared.Next(0, int.MaxValue);

        _builder.WithSchedulePollingRate(value);

        Assert.Equal(value, Configuration.SchedulePollingRate);
    }

    [Fact]
    public void Build_Test()
    {
        _builder
            .WithMaxConcurrentJobs(1)
            .WithSchedulePollingRate(2)
            .WithQueuePollingRate(3)
            .WithNoLogger();

        Assert.Equal(1, Configuration.MaxConcurrentJobs);
        Assert.Equal(2, Configuration.SchedulePollingRate);
        Assert.Equal(3, Configuration.QueuePollingRate);
        Assert.Equal(NullLogger.Instance, Configuration.Logger);
    }
}