namespace Barrel.Exceptions;

public class ImpossibleJobInstantiation<TJob>() : Exception(
    $"Could not instantiate {typeof(TJob)} : " +
    $"register the job via dependency injection, or provide an argument-less constructor, " +
    $"or provide a job instance when scheduling.") where TJob : BaseJob;