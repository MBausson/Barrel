# Barrel

Barrel is a **simple**, **intuitive** and **easy-to-use** background jobs framework.
This project is, for the moment, not designed to be used in production, as it lacks testing and core features.
Additionally, I created this project to improve myself on concurrency and multi-tasking.

As of now, there is no stable release of this project because I want to implement a few more features and add more tests.

**Feedback and contributions are welcomed !**

## Concepts

The idea behind **Barrel** is to provide a very simple yet powerful interface to schedule background jobs.

A job is represented by a sub-class of `BaseJob`. This class defines a `PerformAsync()` method in which the background work is done. The element responsible for scheduling the jobs is a `JobScheduler`.

Example :

```csharp
using var scheduler = new JobScheduler(new JobSchedulerConfigurationBuilder());

//	Schedules a job to be ran in 1 second from now
scheduler.Schedule<SimpleJob>(ScheduleOptions.FromDelay(TimeSpan.FromSeconds(1)));
//	Schedules a job to be ran in 2 seconds from now
scheduler.Schedule<SimpleJob>(ScheduleOptions.FromDelay(TimeSpan.FromSeconds(2)));

//  Schedules a High priority job. Will run before any other job if the concurrent job limit is reached.
//  Default is Medium.
scheduler.Schedule<SimpleJob>(ScheduleOptions.FromPriority(JobPriority.High));

//	Blocking call that waits for all jobs to be complete
await scheduler.WaitAllJobs();
```

### How it's done

Job schedulers use two separate threads to handle incoming and running jobs.

- The first one processes a "Schedule" list, which is a list of scheduled jobs. This thread is responsible for finding jobs that are ready to be executed. When it finds one, the job is placed in a second queue.
- The second thread is responsible for handling the running jobs queue. This queue contains jobs that are waiting to be executed, because there are already too many concurrent jobs. In this queue, jobs with a high Priority are run first. The rate at which jobs are processed in this queue can be changed via the `JobSchedulerConfigurationBuilder.WithMaxConcurrentJobs()` method.
- Jobs are ran within a dedicated `System.Threading.Tasks.Task` object

## Testing

I try to test as many case and feature possible, but it is not an easy task, since we are dealing with concurrency. This mean that on a slower machine, tests might not run as intended, and will probably fail. I'm trying to find workarounds to deal with this issue.

In order to be sure that a test actually fails, please run it several times.

## Future features

- A "persistence extension". This extension will make the JobScheduler use a database to keep track of scheduled jobs, as well as job executions, failure, etc.
- A "dependency injection extension", allowing jobs to use the dependency injection pattern. Such jobs will define the dependencies they use on their constructor.
