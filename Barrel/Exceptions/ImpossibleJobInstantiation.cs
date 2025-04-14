using Barrel.JobData;

namespace Barrel.Exceptions;

public class ImpossibleJobInstantiation(BaseJobData jobData) : Exception(
    $"Could not instantiate {jobData.GetType()} (job id '{jobData.JobId}') : " +
    $"register the job via dependency injection, or provide an argument-less constructor, " +
    $"or provide a job instance when scheduling.");
