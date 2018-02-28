using System.Configuration;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmploymentCheck.Domain.Configuration;
using SFA.DAS.NLog.Logger;


namespace SFA.DAS.EmploymentCheck.AcceptanceTests.DependencyResolution
{
    public class LocalConfiguration
    {
        private readonly string _environment;
        private readonly string _serviceVersion;

        public string PaymentsApiBaseUrl { get; private set; }

        public string CommitmentsApiBaseUrl { get; private set; }
        
        public string AccountsApiBaseUrl { get; private set; }
        
        public string HmrcApiBaseUrl { get; private set; }
        
        public string TokenServiceApiBaseUrl { get; private set; }

        public string EventsApiBaseUrl { get; private set; }

        public string Dbconnectionstring { get; private set; }

        public LocalConfiguration(ILog logger)
        {
            _environment = GetSetting("EnvironmentName");
            _serviceVersion = GetSetting("ServiceVersion");

            var serviceName = GetSetting("ServiceName");
            var tokenServiceName = GetSetting("TokenServiceName");
            var accountApiServiceName = GetSetting("AccountApiServiceName");
            var commitmentsApiServiceName = GetSetting("CommitmentsApiServiceName");
            
            var employmentCheckConfig = GetConfiguration<EmploymentCheckConfiguration>(serviceName);
            var accountApiConfig = GetConfiguration<AccountApiConfiguration>(accountApiServiceName);
            var tokenServiceConfig = GetConfiguration<TokenServiceApiClientConfiguration>(tokenServiceName);
            var commitmentApiConfig = GetConfiguration<CommitmentsApiClientConfiguration>(commitmentsApiServiceName);

            EventsApiBaseUrl = employmentCheckConfig.EventsApi.BaseUrl;
            PaymentsApiBaseUrl = employmentCheckConfig.PaymentsEvents.ApiBaseUrl;
            CommitmentsApiBaseUrl = commitmentApiConfig.BaseUrl;
            TokenServiceApiBaseUrl = tokenServiceConfig.ApiBaseUrl;
            AccountsApiBaseUrl = accountApiConfig.ApiBaseUrl;
            HmrcApiBaseUrl = employmentCheckConfig.HmrcBaseUrl;
            Dbconnectionstring = employmentCheckConfig.DatabaseConnectionString;

            logger.Info($"Events Api baseurl is {EventsApiBaseUrl}");
            logger.Info($"Payments Api baseurl is {PaymentsApiBaseUrl}");
            logger.Info($"Commitments Api baseurl is {CommitmentsApiBaseUrl}");
            logger.Info($"Token Service Api baseurl is {TokenServiceApiBaseUrl}");
            logger.Info($"Accounts Api baseurl is {AccountsApiBaseUrl}");
            logger.Info($"Hmrc Api baseurl is {HmrcApiBaseUrl}");
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
