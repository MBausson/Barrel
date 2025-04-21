using Barrel.Scheduler;
using CalendarJobs;

using var scheduler = new JobScheduler();

DateTimeOffset[] scheduleDateTimes =
[
    DateTimeOffset.UtcNow + TimeSpan.FromSeconds(2),
    DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5),
    DateTimeOffset.UtcNow + TimeSpan.FromSeconds(10)
];

//  Schedules 3 jobs that will respectively be executed 2, 5 and 10 seconds from now.
scheduler.ScheduleCalendar<SimpleJob>(new CalendarScheduleOptions().WithDates(scheduleDateTimes));

await scheduler.WaitAllJobsAsync();
