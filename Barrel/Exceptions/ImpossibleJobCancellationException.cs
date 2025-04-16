using Barrel.JobData;

namespace Barrel.Exceptions;

public class ImpossibleJobCancellationException(BaseJobData jobData) : Exception(
    $"Could not cancel job {jobData.JobClass} (job id '{jobData.JobState}') because " +
    $"its status ({jobData.JobState}) does not permit a cancellation.");
