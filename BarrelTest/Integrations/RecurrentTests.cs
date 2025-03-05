namespace BarrelTest.Integrations;

public class RecurrentTests(ITestOutputHelper output) : IntegrationTest(output)
{
    [Fact]
    public void RecurrentJobFailedScheduleTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Scheduler.ScheduleRecurrent<RecurrentJob>(new RecurrentScheduleOptions().Every(TimeSpan.FromMilliseconds(999))));
    }

    //  Ensures that a recurrent job gets executed every X delay
    [Fact]
    public async Task RecurrentJobExecutedEveryTimeTest()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        Scheduler.ScheduleRecurrent<RecurrentJob>(new RecurrentScheduleOptions().Every(TimeSpan.FromSeconds(1)));
        await WaitForRecurrentExecutions();

        //  Difference between each date time with its next
        var deltas = RecurrentJob.ExecutionDates.Zip(RecurrentJob.ExecutionDates.Skip(1), (a, b) => b - a).ToArray();

        Assert.Equal(3, RecurrentJob.ExecutionsCount);
        Assert.All(deltas, d =>
        {
            var precision = TimeSpan.FromMilliseconds(200);
            var expectedTimeSpan = TimeSpan.FromSeconds(1);

            Assert.InRange(d, expectedTimeSpan - precision, expectedTimeSpan + precision);
        });
    }

    private async Task WaitForRecurrentExecutions()
    {
        await Task.WhenAny(RecurrentJob.JobsFinishedSource.Task, Task.Delay(5000));
    }

    private class RecurrentJob : BaseJob
    {
        public static readonly TaskCompletionSource<bool> JobsFinishedSource = new();
        public static IReadOnlyList<DateTime> ExecutionDates => _executionDates;
        public static int ExecutionsCount => _executionDates.Count;

        private static List<DateTime> _executionDates = new();

        protected override Task PerformAsync()
        {
            _executionDates.Add(DateTime.Now);

            if (ExecutionsCount == 3) JobsFinishedSource.SetResult(true);

            return Task.CompletedTask;
        }
    }
}
