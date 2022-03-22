using SFA.DAS.EmploymentCheck.AcceptanceTests.Hooks;
using SFA.DAS.NServiceBus.Services;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests
{
    public class TestEventPublisher : IEventPublisher
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IHook<object> _hook;

        public TestEventPublisher(IEventPublisher eventPublisher, IHook<object> hook)
        {
            _eventPublisher = eventPublisher;
            _hook = hook;
        }

        public async Task Publish<T>(T message) where T : class
        {
            if (_hook != null)
            {
                try
                {
                    _hook?.OnReceived?.Invoke(message);
                   
                    await _eventPublisher.Publish(message);

                    _hook?.OnProcessed?.Invoke(message);
                }
                catch (Exception ex)
                {
                    var suppressError = false;
                    if (_hook?.OnErrored != null)
                    {
                        suppressError = _hook.OnErrored(ex, message);
                    }
                    if (!suppressError)
                    {
                        throw;
                    }
                }
            }
            else
            {
                await _eventPublisher.Publish(message);
            }
        }

        public Task Publish<T>(Func<T> messageFactory) where T : class
        {
            return Publish(messageFactory.Invoke());
        }
    }
}
