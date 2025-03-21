using Barrel.Scheduler;
using PonctualJobs;

using var scheduler = new JobScheduler();

//  Schedules two job that will respectively be executed 1 and 3 seconds from now.
scheduler.Schedule<SimpleJob>(new ScheduleOptions().WithDelay(TimeSpan.FromSeconds(1)));
scheduler.Schedule<SimpleJob>(new ScheduleOptions().WithDelay(TimeSpan.FromSeconds(3)));

await scheduler.WaitAllJobs();