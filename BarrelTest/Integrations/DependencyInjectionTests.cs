using Barrel.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace BarrelTest.Integrations;

public class DependencyInjectionTests(ITestOutputHelper output) : IntegrationTest(output)
{
    [Fact]
    //  Ensures that we cannot schedule a job if it cannot be instantiated either via DI, or an argument-less
    //  constructor
    public void CannotScheduleWithoutDependencyInjection()
    {
        Scheduler = new JobScheduler(ConfigurationBuilder);

        Assert.Throws<ImpossibleJobInstantiationException<DependentJob>>(() => { Scheduler.Schedule<DependentJob>(); });
    }

    [Fact]
    //  Ensures that a job gets ran when its dependencies have been declared via DI
    public async Task RunJobWithDependencyInjectionTest()
    {
        var services = new ServiceCollection();
        services.AddScoped<IDependency, Dependency>();
        services.AddScoped<DependentJob>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        ConfigurationBuilder.WithDependencyInjection(serviceProvider);
        Scheduler = new JobScheduler(ConfigurationBuilder);

        var jobData = Scheduler.Schedule<DependentJob>();

        await WaitForNonInstancedJobToRun(jobData);

        Assert.True(Dependency.HasWorked);
    }

    private interface IDependency
    {
        public void Work();
    }

    private class Dependency : IDependency
    {
        public static bool HasWorked { get; private set; }

        public void Work()
        {
            HasWorked = true;
        }
    }

    private class DependentJob(IDependency dependency) : TestJob
    {
        protected override Task PerformAsync()
        {
            dependency.Work();

            return base.PerformAsync();
        }
    }
}