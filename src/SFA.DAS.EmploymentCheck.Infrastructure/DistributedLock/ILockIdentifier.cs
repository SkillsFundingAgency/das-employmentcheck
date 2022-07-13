namespace SFA.DAS.EmploymentCheck.Infrastructure.DistributedLock
{
    public interface ILockIdentifier
    {
        string LockId { get; }
    }
}
