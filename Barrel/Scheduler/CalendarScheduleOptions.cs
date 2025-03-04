namespace Barrel.Scheduler;

public class CalendarScheduleOptions : ScheduleOptions
{
    public IReadOnlyList<DateTime> ScheduleDateTimes => _scheduleDateTimes.ToArray();

    private readonly Queue<DateTime> _scheduleDateTimes = new();

    public CalendarScheduleOptions WithDate(DateTime dateTime)
    {
        if (DateTime.Now > dateTime) throw new ArgumentOutOfRangeException($"Datetime {dateTime} is anterior to the current date");

        _scheduleDateTimes.Enqueue(dateTime);

        return this;
    }

    public CalendarScheduleOptions WithDates(params DateTime[] dateTimes)
    {
        foreach (var dateTime in dateTimes) WithDate(dateTime);

        return this;
    }

    public override DateTime NextScheduleOn() => _scheduleDateTimes.Peek();
}
