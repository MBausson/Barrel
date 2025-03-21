using Barrel.Configuration;
using Barrel.Scheduler;
using Microsoft.Extensions.Logging;
using SchedulerConfiguration;

//  Configures a schedule to display all logs, and allow only two jobs to run concurrently.
using var scheduler = new JobScheduler(new JobSchedulerConfigurationBuilder()
    .WithDefaultLogger(LogLevel.Trace)
    .WithMaxConcurrentJobs(2));

scheduler.Schedule<SimpleJob>();
scheduler.Schedule<SimpleJob>();

//  This job will run when the first job will end
scheduler.Schedule<SimpleJob>();

await scheduler.WaitAllJobs();