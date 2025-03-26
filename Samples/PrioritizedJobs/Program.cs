using Barrel;
using Barrel.Configuration;
using Barrel.Scheduler;
using PrioritizedJobs;

using var scheduler = new JobScheduler(new JobSchedulerConfigurationBuilder().WithMaxConcurrentJobs(1));

scheduler.Schedule(new SimpleJob("Medium"));    // 1st scheduled job -- 1st executed

//  We need this delay to demonstrate how the priority works
//  Without this, the High job gets executed first
//  Because we schedule these three jobs so fast, the scheduler doesn't have the time to
//      execute the 1st medium job
//  Messing around with QueuePollingRate might fix this
await Task.Delay(200);

scheduler.Schedule(new SimpleJob("Medium"));    // 2nd scheduled job -- 3rd executed
scheduler.Schedule(new SimpleJob("High"), ScheduleOptions.FromPriority(JobPriority.High)); // 3rd scheduled job -- 2nd executed

await scheduler.WaitAllJobs();
//  We have specified that our scheduler should, at most, run one job concurrently.
//  The first job will run normally, but the next job will be third one, since its priority is higher than the 2nd job

//  Priority matters only when the maximum amount of concurrent jobs is reached. If we set this option to a higher
//      number, the 2nd job will run before the 3rd one.
