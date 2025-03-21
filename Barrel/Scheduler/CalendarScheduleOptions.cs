namespace Barrel.Scheduler;

public class CalendarScheduleOptions : ScheduleOptions
{
    private readonly SortedList<DateTime, DateTime> _scheduleDateTimes = new();
    public IReadOnlyList<DateTime> ScheduleDateTimes => _scheduleDateTimes.Values.ToArray();

    public CalendarScheduleOptions WithDate(DateTime dateTime)
    {
        if (DateTime.Now > dateTime)
            throw new ArgumentOutOfRangeException($"Datetime {dateTime} is anterior to the current date");

        _scheduleDateTimes.Add(dateTime, dateTime);

        return this;
    }

    public CalendarScheduleOptions WithDates(params DateTime[] dateTimes)
    {
        foreach (var dateTime in dateTimes) WithDate(dateTime);

        return this;
    }

    public override DateTime NextScheduleOn()
    {
        return _scheduleDateTimes.First().Value;
    }
}