namespace Barrel.Scheduler;

public class CalendarScheduleOptions : RecurrentScheduleOptions
{
    public IReadOnlyList<DateTime> SchedulesDatetime => _schedulesDatetime;

    private List<DateTime> _schedulesDatetime = new();

    public CalendarScheduleOptions WithDate(DateTime dateTime)
    {
        if (DateTime.Now > dateTime) throw new ArgumentOutOfRangeException($"Datetime {dateTime} is anterior to the current date");

        _schedulesDatetime.Add(dateTime);

        return this;
    }

    public CalendarScheduleOptions WithDates(params DateTime[] dateTimes)
    {
        foreach (var dateTime in dateTimes) WithDate(dateTime);

        return this;
    }

    public Queue<DateTime> ToSchedulesQueue()
    {
        var queue = new Queue<DateTime>();

        _schedulesDatetime.ForEach(d => queue.Enqueue(d));

        return queue;
    }
}
