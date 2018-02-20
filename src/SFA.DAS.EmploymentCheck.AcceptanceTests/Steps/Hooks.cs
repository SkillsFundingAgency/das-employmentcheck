using System.Threading.Tasks;
using TechTalk.SpecFlow;
using BoDi;
using SFA.DAS.EmploymentCheck.SubmissionEventWorkerRole;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Infrastructure;
using SFA.DAS.AccountsApiSubstitute.WebAPI;
using SFA.DAS.EmploymentCheck.AcceptanceTests.DependencyResolution;
using SFA.DAS.ProviderEventsApiSubstitute.WebAPI;
using SFA.DAS.CommitmentsApiSubstitute.WebAPI;
using SFA.DAS.HmrcApiSubstitute.WebAPI;
using SFA.DAS.ApiSubstitute.WebAPI;
using SFA.DAS.ApiSubstitute.WebAPI.MessageHandlers;


namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    public class Hooks
    {
        private IObjectContainer _objectContainer;

        private WorkerRole sut;

        private AccountsApi AccountsApi;

        private ProviderEventsApi ProviderEventsApi;
        
        private CommitmentsApi CommitmentsApi;

        private HmrcApi HmrcApi;

        private WebApiSubstitute EventsApiSubstitute;
        
        private WebApiSubstitute TokenApiSubstitute;


        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }
        
        [BeforeScenario]
        public async Task BeforeScenario()
        {
            var config = new LocalConfiguration();
            AccountsApiMessageHandler accountsApiMessageHandlers = new AccountsApiMessageHandler(config.AccountsApiBaseUrl);
            AccountsApi = new AccountsApi(accountsApiMessageHandlers);

            ProviderEventsApiMessageHandler providerApiMessageHandlers = new ProviderEventsApiMessageHandler(config.PaymentsApiBaseUrl);
            ProviderEventsApi = new ProviderEventsApi(providerApiMessageHandlers);

            CommitmentsApiMessageHandler commitmentsApiMessageHandlers = new CommitmentsApiMessageHandler(config.CommitmentsApiBaseUrl);
            CommitmentsApi = new CommitmentsApi(commitmentsApiMessageHandlers);

            HmrcApiMessageHandler hmrcApiMessageHandlers = new HmrcApiMessageHandler(config.HmrcApiBaseUrl);
            HmrcApi = new HmrcApi(hmrcApiMessageHandlers);

            ApiMessageHandlers eventsApiMessageHandlers = new ApiMessageHandlers(config.EventsApiBaseUrl);
            EventsApiSubstitute = new WebApiSubstitute(eventsApiMessageHandlers);
            
            ApiMessageHandlers tokenApiMessageHandlers = new ApiMessageHandlers(config.TokenServiceApiBaseUrl);
            TokenApiSubstitute = new WebApiSubstitute(tokenApiMessageHandlers);

            EmploymentCheckRepository employmentCheckRepository = new EmploymentCheckRepository(config.Dbconnectionstring);

            sut = new WorkerRole();
            sut.OnStart();

            _objectContainer.RegisterInstanceAs(sut);
            _objectContainer.RegisterInstanceAs(accountsApiMessageHandlers);
            _objectContainer.RegisterInstanceAs(providerApiMessageHandlers);
            _objectContainer.RegisterInstanceAs(commitmentsApiMessageHandlers);
            _objectContainer.RegisterInstanceAs(hmrcApiMessageHandlers);
            _objectContainer.RegisterInstanceAs(eventsApiMessageHandlers,"eventsapi");
            _objectContainer.RegisterInstanceAs(tokenApiMessageHandlers,"tokenserviceapi");
            _objectContainer.RegisterInstanceAs(employmentCheckRepository);

            //Clean Database
            await employmentCheckRepository.SetLastProcessedEventId();
            await employmentCheckRepository.RemoveSubmissionEvents();
            
        }

        [AfterScenario]
        public void AfterScenario()
        {
            sut?.OnStop();
            AccountsApi?.Dispose();
            ProviderEventsApi?.Dispose();
            CommitmentsApi?.Dispose();
            HmrcApi?.Dispose();
            EventsApiSubstitute?.Dispose();
            TokenApiSubstitute?.Dispose();
        }
    }
}
