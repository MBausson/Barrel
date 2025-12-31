using Barrel.JobData;

namespace Barrel.Exceptions;

public class JobOperationNotPermitted(BaseJobData jobData) : Exception(
    $"Job {jobData.Id} ({jobData.JobClass}) could not receive requested operation " +
    $"because its state ({jobData.State}) does not permit it");
