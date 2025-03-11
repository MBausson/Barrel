namespace Barrel.Scheduler;

public class CalendarScheduleOptions : ScheduleOptions
{
    public IReadOnlyList<DateTime> ScheduleDateTimes => _scheduleDateTimes.Values.ToArray();

    private readonly SortedList<DateTime, DateTime> _scheduleDateTimes = new();

    public CalendarScheduleOptions WithDate(DateTime dateTime)
    {
        if (DateTime.Now > dateTime) throw new ArgumentOutOfRangeException($"Datetime {dateTime} is anterior to the current date");

        _scheduleDateTimes.Add(dateTime, dateTime);

        return this;
    }

    public CalendarScheduleOptions WithDates(params DateTime[] dateTimes)
    {
        foreach (var dateTime in dateTimes) WithDate(dateTime);

        return this;
    }

    public override DateTime NextScheduleOn() => _scheduleDateTimes.First().Value;
}
