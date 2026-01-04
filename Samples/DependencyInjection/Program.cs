using Barrel.Configuration;
using Barrel.Scheduler;
using DependencyInjection;
using DependencyInjection.Services;
using Microsoft.Extensions.DependencyInjection;

//  Register needed services, as well as the job that will use DI
var services = new ServiceCollection();
services.AddScoped<ISimpleTask, SimpleTask>();
services.AddScoped<SimpleJob>();

IServiceProvider serviceProvider = services.BuildServiceProvider();

//  Specify which service provider to use
using var scheduler = new JobScheduler(new JobSchedulerConfigurationBuilder()
    .WithDependencyInjection(serviceProvider));

//  If we do not pass a SimpleJob instance, Barrel will try to create one with DI
scheduler.ScheduleRecurrent<SimpleJob>(new RecurrentScheduleOptions().Every(TimeSpan.FromSeconds(5)));

await scheduler.WaitAllJobsAsync();

// The SimpleJob will be instantiated via Dependency Injection, along with its dependencies.