using System.Configuration;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmploymentCheck.Domain.Configuration;


namespace SFA.DAS.EmploymentCheck.AcceptanceTests.DependencyResolution
{
    public class LocalConfiguration
    {
        private readonly string _environment;
        private readonly string _serviceName;
        private readonly string _tokenServiceName;
        private readonly string _accountApiServiceName;
        private readonly string _commitmentsApiServiceName;
        private readonly string _serviceVersion;

        public string PaymentsApiBaseUrl { get; private set; }

        public string CommitmentsApiBaseUrl { get; private set; }
        
        public string AccountsApiBaseUrl { get; private set; }
        
        public string HmrcApiBaseUrl { get; private set; }
        
        public string TokenServiceApiBaseUrl { get; private set; }

        public string EventsApiBaseUrl { get; private set; }

        public string Dbconnectionstring { get; private set; }

        public LocalConfiguration()
        {
            _environment = GetSetting("EnvironmentName");
            _serviceName = GetSetting("ServiceName");
            _tokenServiceName = GetSetting("TokenServiceName");
            _accountApiServiceName = GetSetting("AccountApiServiceName");
            _commitmentsApiServiceName = GetSetting("CommitmentsApiServiceName");
            _serviceVersion = GetSetting("ServiceVersion");

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

        private static string GetSetting(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? string.Empty : value;
        }

    }
}
