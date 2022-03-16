using System;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Hooks
{
    public interface IHook
    {
    }
    public interface IHook<T> : IHook
    {
        Action<T> OnReceived { get; set; }
        Action<T> OnPublished { get; set; }
        Action<T> OnProcessed { get; set; }
        Action<T> OnDelayed { get; set; }
        Action<T> OnHandled { get; set; }
        /// <summary>
        /// return true to suppress the raising of the exception
        /// </summary>
        Func<Exception, T, bool> OnErrored { get; set; }
    }
}
