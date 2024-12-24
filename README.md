# Barrel

Barrel is a **simple**, **intuitive** and **easy-to-use** background jobs framework.  
This project is, for the moment, not designed to be used in production, as it lacks testing and core features.
Furthermore, I created this project to train myself on concurrency & threading.  

## Concepts

The idea behind **Barrel** is to provide a very simple and concise interface to schedule background jobs.  
Thus, using this framework consists of working with (essentially) two components.

### BaseJob

This is the base class used to define jobs. This class requires the implementation of a `Perform()` method, in which the actual work is done. 
This class also gives access to some informations about the job, such as the ID, its state or even its priority level.  

### JobScheduler

This class does all the magic : you will use it to schedule future jobs.  

## Usage

First, let's create a scheduler.  
By default, the scheduler uses at most 5 threads to run concurrently its enqueued jobs. This value can be changed via the `JobSchedulerConfiguration` instanced passed in its constructor.  

```cs
JobScheduler scheduler = new JobScheduler(new JobSchedulerConfiguration());
```

If the scheduler's thread pool size reaches its limit, any new scheduled jobs will be enqueued and wait for the running jobs to free a thread. This behaviour is tuned according to jobs's priorities :

* **Low priority** - Jobs with this priority will run if no other jobs with a higher priority has been enqueued.  
* **Medium priority** - Jobs with this priority will run before any Low enqueued jobs.  
* **High priority** - Jobs with this priority will pause other running jobs if no thread is available.  

The priority is defined as a `BaseJob` class property.  
**Note:** This behaviour isn't implemented yet, and will likely be reworked.  

Let's schedule some jobs :

```cs
scheduler.Schedule<SimpleJob>(TimeSpan.FromDays(2));
scheduler.Schedule(new SimpleJob(), TimeSpan.FromSeconds(5));
```

In this code, we schedule two `SimpleJob` jobs to run with a defined delay.  

Notice the syntax used : for the first schedule, we do **not** specify any `SimpleJob` instance, in contrast to the second schedule.  
Since jobs can be scheduled to run in a long time (for example, in **6 months**), it does not make sense to keep an instance of a job in memory for such a long time. Instead, the class will be instantiated just before the job gets ran. This syntax requires a parameter-less constructor for the `BaseJob` used.
