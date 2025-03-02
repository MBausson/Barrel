namespace BarrelTest.Units;

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
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _options.Every(TimeSpan.FromMilliseconds(999));
        });
    }
}
