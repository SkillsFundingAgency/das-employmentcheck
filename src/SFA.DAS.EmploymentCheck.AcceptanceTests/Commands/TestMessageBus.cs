using System.IO;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EmploymentCheck.Commands;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Commands
{
    public class TestMessageBus
    {
        private readonly TestContext _testContext;
        private IEndpointInstance _endpointInstance;
        public bool IsRunning { get; private set; }
        public DirectoryInfo StorageDirectory { get; private set; }

        public TestMessageBus(TestContext testContext)
        {
            _testContext = testContext;
        }

        public async Task Start()
        {
            StorageDirectory = new DirectoryInfo(Path.Combine(_testContext.TestDirectory.FullName, ".learningtransport"));
            if (!StorageDirectory.Exists)
            {
                Directory.CreateDirectory(StorageDirectory.FullName);
            }
            
            var endpointConfiguration = new EndpointConfiguration(_testContext.InstanceId);
            endpointConfiguration                
                .UseNewtonsoftJsonSerializer()
                .UseMessageConventions()
                .UseTransport<LearningTransport>()                
                .StorageDirectory(StorageDirectory.FullName);

            endpointConfiguration.UseLearningTransport(s => s.AddRouting());

            _endpointInstance = await Endpoint.Start(endpointConfiguration);
            IsRunning = true;
        }

        public async Task Stop()
        {
            await _endpointInstance.Stop().ConfigureAwait(false);
            IsRunning = false;
        }

        public Task Publish(object message)
        {            
            return _endpointInstance.Publish(message);
        }

        public Task Send(object message)
        {
            return _endpointInstance.Send(message);
        }
    }
}
