using Ardalis.GuardClauses;
using Newtonsoft.Json;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount
{
    public class EmployerAccountService : IEmployerAccountService
    {
        private readonly IEncodingService _encodingService;
        private readonly IAccountsResponseRepository _repository;
        private readonly IEmployerAccountApiClient<EmployerAccountApiConfiguration> _apiClient;
        private readonly IApiRetryPolicies _apiRetryPolicies;

        public EmployerAccountService(
            IEncodingService encodingService,
            IAccountsResponseRepository repository,
            IEmployerAccountApiClient<EmployerAccountApiConfiguration> apiClient,
            IApiRetryPolicies apiRetryPolicies
        )
        {
            _encodingService = encodingService;
            _repository = repository;
            _apiClient = apiClient;
            _apiRetryPolicies = apiRetryPolicies;
        }

        public async Task<EmployerPayeSchemes> GetEmployerPayeSchemes(Data.Models.EmploymentCheck employmentCheck)
        {
            HttpResponseMessage response = null;

            try
            {
                
                var hashedAccountId = _encodingService.Encode(employmentCheck.AccountId, EncodingType.AccountId);
                var request = new GetAccountPayeSchemesRequest(hashedAccountId);

                var policy = await _apiRetryPolicies.GetAll("AccountApiKey");
                response = await _apiClient.GetWithPolicy(policy, request);

                return await ProcessPayeSchemesFromApiResponse(employmentCheck, response);
            }
            catch (Exception ex)
            {
                await HandleException(employmentCheck, ex);
                return null;
            }
        }

        private async Task<EmployerPayeSchemes> ProcessPayeSchemesFromApiResponse(Data.Models.EmploymentCheck employmentCheck, HttpResponseMessage httpResponseMessage)
        {
            Guard.Against.Null(employmentCheck, nameof(employmentCheck));

            if (httpResponseMessage == null)
            {
                await Save(CreateResponseModel(employmentCheck));
                return null;
            }

            var response = CreateResponseModel(employmentCheck, httpResponseMessage.ToString(), httpResponseMessage.StatusCode);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                await Save(response);
            }

            var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            try
            {
                var employerPayeSchemes = DeserialiseContent(jsonContent, response);

                response.SetPayeSchemes(employerPayeSchemes?.PayeSchemes);

                await Save(response);

                return employerPayeSchemes;
            }
            catch(Exception)
            {
                return new EmployerPayeSchemes(employmentCheck.AccountId, (HttpStatusCode)response.HttpStatusCode);
            }
        }

        private static AccountsResponse CreateResponseModel(Data.Models.EmploymentCheck employmentCheck, string httpResponseMessage = null, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            return AccountsResponse.CreateResponse(
                employmentCheck.Id,
                employmentCheck.CorrelationId,
                employmentCheck.AccountId,
                httpResponseMessage,
                (short)statusCode);
        }

        private static EmployerPayeSchemes DeserialiseContent(string jsonContent, AccountsResponse accountsResponse)
        {
            Guard.Against.Null(accountsResponse, nameof(accountsResponse));

            if (!string.IsNullOrEmpty(jsonContent))
            {
                var resourceList = JsonConvert.DeserializeObject<ResourceList>(jsonContent);
                if (resourceList != null && resourceList.Any())
                {
                    return new EmployerPayeSchemes(accountsResponse.AccountId, (HttpStatusCode)accountsResponse.HttpStatusCode, resourceList.Select(x => x.Id.Trim().ToUpper()).ToList());
                }
            }

            return new EmployerPayeSchemes(accountsResponse.AccountId, (HttpStatusCode)accountsResponse.HttpStatusCode, null);
        }

        private async Task HandleException(Data.Models.EmploymentCheck employmentCheck, Exception e)
        {
            var accountsResponse = CreateResponseModel(employmentCheck, e.Message);

            await Save(accountsResponse);
        }

        private async Task Save(AccountsResponse accountsResponse)
        {
            await _repository.InsertOrUpdate(accountsResponse);
        }
    }
}