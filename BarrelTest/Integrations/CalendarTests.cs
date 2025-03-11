namespace BarrelTest.Integrations;

public class CalendarTests(ITestOutputHelper output) : IntegrationTest(output)
{
    [Fact]
    public void NoDateScheduledTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        Assert.Throws<InvalidOperationException>(
            () => Scheduler.ScheduleCalendar<CalendarJob>(new CalendarScheduleOptions()));
    }

    [Fact]
    public async Task DatesScheduledTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        DateTime[] executionDates = [
            DateTime.Now + TimeSpan.FromMilliseconds(1000),
            DateTime.Now + TimeSpan.FromMilliseconds(1500),
            DateTime.Now + TimeSpan.FromMilliseconds(2000)
        ];

        Scheduler.ScheduleCalendar<CalendarJob>(new CalendarScheduleOptions()
            .WithDate(executionDates[0])
            .WithDate(executionDates[1])
            .WithDate(executionDates[2]));

        await WaitForRecurrentExecutions();

        Assert.Equal(3, CalendarJob.ExecutionsCount);
        Assert.All(CalendarJob.ExecutionDateTimes, (date, i) =>
        {
            Assert.Equal(executionDates[i], date, TimeSpan.FromMilliseconds(500));
        });
    }

    private async Task WaitForRecurrentExecutions()
    {
        await Task.WhenAny(CalendarJob.JobsFinishedSource.Task, Task.Delay(10000));
    }

    private class CalendarJob : BaseJob
    {
        public static readonly TaskCompletionSource<bool> JobsFinishedSource = new();
        public static IReadOnlyList<DateTime> ExecutionDateTimes => ExecutionDates;
        public static int ExecutionsCount => ExecutionDateTimes.Count;

        private static readonly List<DateTime> ExecutionDates = new();

        protected override Task PerformAsync()
        {
            ExecutionDates.Add(DateTime.Now);

            if (ExecutionsCount == 3) JobsFinishedSource.SetResult(true);

            return Task.CompletedTask;
        }
    }
}
