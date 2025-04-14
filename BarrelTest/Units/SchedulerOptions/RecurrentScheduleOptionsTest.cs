namespace BarrelTest.Units.SchedulerOptions;

public class RecurrentScheduleOptionsTest
{
    private readonly RecurrentScheduleOptions _options = new();

    [Fact]
    public void Every_SetsPeriodicityTest()
    {
        var everySeconds = Random.Shared.Next(1, 3600);

        _options.Every(TimeSpan.FromSeconds(everySeconds));

        Assert.Equal(everySeconds, _options.Periodicity.TotalSeconds);
    }

    [Fact]
    public void Every_TooShortDelayTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => { _options.Every(TimeSpan.FromMilliseconds(999)); });
    }

    //  Ensures that NextScheduleOn returns the current datetime when no delay is applied
    [Fact]
    public void NextScheduleOn_NoPeriodicity()
    {
        Assert.Equal(DateTime.Now, _options.NextScheduleOn(), TimeSpan.FromMilliseconds(999));
    }

    //  Ensures that NextScheduleOn returns the valid datetime when delay is applied
    [Fact]
    public void NextScheduleOn_WithPeriodicityAndNoDelayTest()
    {
        var secondsPeriodicity = Random.Shared.NextInt64(1, 3600);

        _options.Every(TimeSpan.FromSeconds(secondsPeriodicity));

        Assert.Equal(DateTime.Now + TimeSpan.FromSeconds(secondsPeriodicity), _options.NextScheduleOn(),
            TimeSpan.FromMilliseconds(999));
    }

    //  Ensures that NextScheduleOn returns the valid datetime when delay is applied
    [Fact]
    public void NextScheduleOn_WithPeriodicityAndDelayTest()
    {
        var secondsDelay = Random.Shared.NextInt64(1, 3600);
        var secondsPeriodicity = Random.Shared.NextInt64(1, 3600);

        _options.WithDelay(TimeSpan.FromSeconds(secondsDelay));
        _options.Every(TimeSpan.FromSeconds(secondsPeriodicity));

        var expectedDate = DateTime.Now + TimeSpan.FromSeconds(secondsDelay) + TimeSpan.FromSeconds(secondsPeriodicity);

        Assert.Equal(expectedDate, _options.NextScheduleOn(), TimeSpan.FromMilliseconds(999));
    }
}
