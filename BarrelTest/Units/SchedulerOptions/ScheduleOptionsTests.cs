namespace BarrelTest.Units.SchedulerOptions;

public class ScheduleOptionsTests
{
    private readonly ScheduleOptions _options = new();

    //  Ensures that WithDelay adds the right amount of delay
    [Fact]
    public void WithDelay_AddsDelayTest()
    {
        var newDelay = TimeSpan.FromSeconds(Random.Shared.NextInt64(100, 1000));

        _options.WithDelay(newDelay);

        Assert.Equal(newDelay, _options.Delay);
    }

    //  Ensures that a new object is created with the right amount of delay
    [Fact]
    public void FromDelay_AddsDelayTest()
    {
        var newDelay = TimeSpan.FromSeconds(new Random().NextInt64(100, 1000));

        Assert.Equal(newDelay, ScheduleOptions.FromDelay(newDelay).Delay);
    }

    //  Ensures that WithPriority sets the right priority
    [Fact]
    public void WithPriority_SetsPriorityTest()
    {
        var priority = (JobPriority)Random.Shared.NextInt64(3);

        _options.WithPriority(priority);

        Assert.Equal(priority, _options.Priority);
    }

    //  Ensures that FromPriority creates a new object with the right priority
    [Fact]
    public void FromPriority_SetsPriorityTest()
    {
        var priority = (JobPriority)Random.Shared.NextInt64(3);

        Assert.Equal(priority, ScheduleOptions.FromPriority(priority).Priority);
    }

    //  Ensures that WithMaxRetries rejects negative values, except -1
    [Fact]
    public void WithMaxRetries_NegativeInputTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _options.WithMaxRetries(Random.Shared.Next(int.MinValue, -1));
        });
    }

    //  Ensures that WithMaxRetries accepts -1
    [Fact]
    public void WithMaxRetries_NegativeOneInput_SetsMaxRetriesTest()
    {
        _options.WithMaxRetries(-1);

        Assert.Equal(-1, _options.MaxRetries);
    }

    //  Ensures that WithMaxRetries sets the MaxRetries property
    [Fact]
    public void WithMaxRetries_SetsMaxRetriesTest()
    {
        var value = Random.Shared.Next(0, int.MaxValue);

        _options.WithMaxRetries(value);

        Assert.Equal(value, _options.MaxRetries);
    }

    //  Ensures that NextScheduleOn returns the current datetime when no delay is applied
    [Fact]
    public void NextScheduleOn_NoDelayTest()
    {
        Assert.Equal(DateTime.Now, _options.NextScheduleOn(), TimeSpan.FromMilliseconds(999));
    }

    //  Ensures that NextScheduleOn returns the valid datetime when delay is applied
    [Fact]
    public void NextScheduleOn_WithDelayTest()
    {
        var secondsDelay = Random.Shared.NextInt64(1, 3600);

        _options.WithDelay(TimeSpan.FromSeconds(secondsDelay));

        Assert.Equal(DateTime.Now + TimeSpan.FromSeconds(secondsDelay), _options.NextScheduleOn(),
            TimeSpan.FromMilliseconds(999));
    }
}
