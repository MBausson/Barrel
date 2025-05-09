﻿namespace Barrel;

/// <summary>
///     The job priority helps the JobScheduler to know when to start a Job.
/// </summary>
public enum JobPriority
{
    /// <summary>
    ///     Will default to Medium priority if no priority is given on a job instance.
    /// </summary>
    Default,

    /// <summary>
    ///     Jobs with this priority will run if no other jobs with a higher priority has been enqueued.
    /// </summary>
    Low,

    /// <summary>
    ///     Jobs with this priority will run before any <c>Low</c> enqueued job.
    /// </summary>
    Medium,

    /// <summary>
    ///     Jobs with this priority will run before any non-high job.
    /// </summary>
    High
}