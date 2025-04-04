using Barrel.Configuration;
using Barrel.Scheduler;
using DependencyInjection;
using DependencyInjection.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddScoped<ISimpleTask, SimpleTask>();
services.AddScoped<SimpleJob>();

IServiceProvider serviceProvider = services.BuildServiceProvider();

using var scheduler = new JobScheduler(new JobSchedulerConfigurationBuilder().WithDependencyInjection(serviceProvider));

scheduler.ScheduleDependencyInjection<SimpleJob>(new());

await scheduler.WaitAllJobs();
