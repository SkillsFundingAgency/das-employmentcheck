using TechTalk.SpecFlow;
using BoDi;
using SFA.DAS.AccountsApiSubstitute.WebAPI;
using SFA.DAS.ProviderEventsApiSubstitute.WebAPI;
using SFA.DAS.CommitmentsApiSubstitute.WebAPI;
using SFA.DAS.HmrcApiSubstitute.WebAPI;
using SFA.DAS.EventsApiSubstitute.WebAPI;
using SFA.DAS.TokenServiceApiSubstitute.WebAPI;
using SFA.DAS.EmploymentCheck.AcceptanceTests.DependencyResolution;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    public class RegisterApi
    {
        private IObjectContainer _objectContainer;

        private AccountsApi AccountsApi;

        private ProviderEventsApi ProviderEventsApi;

        private CommitmentsApi CommitmentsApi;

        private HmrcApi HmrcApi;

        private EventsApi EventsApiSubstitute;

        private TokenServiceApi TokenApiSubstitute;

        public RegisterApi(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [BeforeScenario(Order = 2)]
        public void BeforeScenario()
        {
            var config = _objectContainer.Resolve<LocalConfiguration>();

            AccountsApiMessageHandler accountsApiMessageHandlers = new AccountsApiMessageHandler(config.AccountsApiBaseUrl);
            AccountsApi = new AccountsApi(accountsApiMessageHandlers);

            ProviderEventsApiMessageHandler providerApiMessageHandlers = new ProviderEventsApiMessageHandler(config.PaymentsApiBaseUrl);
            ProviderEventsApi = new ProviderEventsApi(providerApiMessageHandlers);

            CommitmentsApiMessageHandler commitmentsApiMessageHandlers = new CommitmentsApiMessageHandler(config.CommitmentsApiBaseUrl);
            CommitmentsApi = new CommitmentsApi(commitmentsApiMessageHandlers);

            EventsApiMessageHandler eventsApiMessageHandlers = new EventsApiMessageHandler(config.EventsApiBaseUrl);
            EventsApiSubstitute = new EventsApi(eventsApiMessageHandlers);

            TokenServiceApiMessageHandler tokenApiMessageHandlers = new TokenServiceApiMessageHandler(config.TokenServiceApiBaseUrl);
            TokenApiSubstitute = new TokenServiceApi(tokenApiMessageHandlers);

            HmrcApiMessageHandler hmrcApiMessageHandlers = new HmrcApiMessageHandler(config.HmrcApiBaseUrl);
            HmrcApi = new HmrcApi(hmrcApiMessageHandlers);

            _objectContainer.RegisterInstanceAs(accountsApiMessageHandlers);
            _objectContainer.RegisterInstanceAs(providerApiMessageHandlers);
            _objectContainer.RegisterInstanceAs(commitmentsApiMessageHandlers);
            _objectContainer.RegisterInstanceAs(hmrcApiMessageHandlers);
            _objectContainer.RegisterInstanceAs(eventsApiMessageHandlers);
            _objectContainer.RegisterInstanceAs(tokenApiMessageHandlers);
            
        }

        [AfterScenario(Order = 2)]
        public void AfterScenario()
        {
            AccountsApi?.Dispose();
            ProviderEventsApi?.Dispose();
            CommitmentsApi?.Dispose();
            HmrcApi?.Dispose();
            EventsApiSubstitute?.Dispose();
            TokenApiSubstitute?.Dispose();
        }
    }
}
