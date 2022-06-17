using System;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Hooks
{
    public class Hook<T> : IHook<T>
    {
        public Action<T> OnReceived { get; set; }
        public Action<T> OnPublished { get; set; }
        public Action<T> OnProcessed { get; set; }
        public Action<T> OnDelayed { get; set; }
        public Action<T> OnHandled { get; set; }
        public Func<Exception, T, bool> OnErrored { get; set; }
    }
}
