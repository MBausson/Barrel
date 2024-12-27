using Barrel;
using Barrel.Scheduler;

namespace BarrelTest.Units;

public class ScheduleOptionsTests
{
    private ScheduleOptions _options = new();

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
}
