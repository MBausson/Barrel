namespace BarrelTest.Units.SchedulerOptions;

public class CalendarScheduleOptionsTest
{
    private readonly CalendarScheduleOptions _options = new();

    [Fact]
    public void WithDate_WithValidDateAddsDateTest()
    {
        var date1 = DateTimeOffset.UtcNow + TimeSpan.FromHours(Random.Shared.Next(1, 24));

        _options.WithDate(date1);

        Assert.Equivalent(new[] { date1 }, _options.ScheduleDateTimes);
        Assert.Equal(date1, _options.NextScheduleOn());
    }

    [Fact]
    public void WithDate_WithAnteriorDateTest()
    {
        var date1 = DateTimeOffset.UtcNow - TimeSpan.FromSeconds(Random.Shared.Next(1, 3600));

        Assert.Throws<ArgumentOutOfRangeException>(() => _options.WithDate(date1));
    }

    [Fact]
    public void WithDates_WithValidDatesAddsDatesTest()
    {
        var date1 = DateTimeOffset.UtcNow + TimeSpan.FromHours(Random.Shared.Next(1, 24));
        var date2 = DateTimeOffset.UtcNow + TimeSpan.FromHours(Random.Shared.Next(1, 24));
        var date3 = DateTimeOffset.UtcNow + TimeSpan.FromHours(Random.Shared.Next(1, 24));

        _options.WithDates(date1, date2, date3);

        Assert.Equivalent(new[] { date1, date2, date3 }, _options.ScheduleDateTimes);
    }

    [Fact]
    public void WithDates_WithOneAnteriorDateTest()
    {
        var date1 = DateTimeOffset.UtcNow + TimeSpan.FromHours(Random.Shared.Next(1, 24));
        var date2 = DateTimeOffset.UtcNow - TimeSpan.FromHours(Random.Shared.Next(1, 24));
        var date3 = DateTimeOffset.UtcNow + TimeSpan.FromHours(Random.Shared.Next(1, 24));

        Assert.Throws<ArgumentOutOfRangeException>(() => _options.WithDates(date1, date2, date3));
    }

    [Fact]
    public void NextScheduleOn_ReturnsEarliestDateTest()
    {
        var date1 = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(Random.Shared.Next(1, 59));
        var date2 = DateTimeOffset.UtcNow + TimeSpan.FromHours(Random.Shared.Next(1, 23));
        var date3 = DateTimeOffset.UtcNow + TimeSpan.FromDays(Random.Shared.Next(1, 30));

        _options.WithDates(date1, date2, date3);

        Assert.Equal(date1, _options.NextScheduleOn());
    }
}
