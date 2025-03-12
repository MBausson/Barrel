using Barrel.Scheduler;
using RecurrentJobs;

using var scheduler = new JobScheduler();

//  SimpleJob will be executed every 3 seconds
scheduler.ScheduleRecurrent<SimpleJob>(new RecurrentScheduleOptions().Every(TimeSpan.FromSeconds(3)));

await scheduler.WaitAllJobs();
