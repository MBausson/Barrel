using Barrel.JobData;

namespace Barrel.Exceptions;

public class ImpossibleJobCancellationException(BaseJobData jobData) : Exception(
    $"Could not cancel job {jobData.JobClass} (job id '{jobData.State}') because " +
    $"its status ({jobData.State}) does not permit a cancellation.");