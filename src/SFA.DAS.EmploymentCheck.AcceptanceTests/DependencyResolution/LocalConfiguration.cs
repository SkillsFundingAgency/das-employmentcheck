using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmploymentCheck.Domain.Configuration;

namespace SFA.DAS.EmploymentCheck.UserAcceptanceTests.DependencyResolution
{
    public class LocalConfiguration
    {
        private readonly string _environment = CloudConfigurationManager.GetSetting("EnvironmentName");
        private readonly string _serviceName = CloudConfigurationManager.GetSetting("ServiceName");
        private readonly string _tokenServiceName = CloudConfigurationManager.GetSetting("TokenServiceName");
        private readonly string _accountApiServiceName = CloudConfigurationManager.GetSetting("AccountApiServiceName");
        private readonly string _commitmentsApiServiceName = CloudConfigurationManager.GetSetting("CommitmentsApiServiceName");
        private readonly string _serviceVersion = CloudConfigurationManager.GetSetting("ServiceVersion");

        public string PaymentsApiBaseUrl { get; private set; }

        public string CommitmentsApiBaseUrl { get; private set; }
        
        public string AccountsApiBaseUrl { get; private set; }
        
        public string HmrcApiBaseUrl { get; private set; }
        
        public string TokenServiceApiBaseUrl { get; private set; }

        public string EventsApiBaseUrl { get; private set; }

        public string Dbconnectionstring { get; private set; }

        public LocalConfiguration()
        {
            var employmentCheckConfig = GetConfiguration<EmploymentCheckConfiguration>(_serviceName);
            var accountApiConfig = GetConfiguration<AccountApiConfiguration>(_accountApiServiceName);
            var tokenServiceConfig = GetConfiguration<TokenServiceApiClientConfiguration>(_tokenServiceName);
            var commitmentApiConfig = GetConfiguration<CommitmentsApiClientConfiguration>(_commitmentsApiServiceName);

            EventsApiBaseUrl = employmentCheckConfig.EventsApi.BaseUrl;
            PaymentsApiBaseUrl = employmentCheckConfig.PaymentsEvents.ApiBaseUrl;
            CommitmentsApiBaseUrl = commitmentApiConfig.BaseUrl;
            TokenServiceApiBaseUrl = tokenServiceConfig.ApiBaseUrl;
            AccountsApiBaseUrl = accountApiConfig.ApiBaseUrl;
            HmrcApiBaseUrl = employmentCheckConfig.HmrcBaseUrl;
            Dbconnectionstring = employmentCheckConfig.DatabaseConnectionString;
        }

        private T GetConfiguration<T>(string serviceName)
        {
            var configurationRepository = new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
            var configurationService = new ConfigurationService(configurationRepository, new ConfigurationOptions(serviceName, _environment, _serviceVersion));
            return configurationService.Get<T>();
        }
    }
}
