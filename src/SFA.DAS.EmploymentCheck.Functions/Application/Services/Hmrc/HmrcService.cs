using Dapper;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;
using System;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using Ardalis.GuardClauses;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public class HmrcService : IHmrcService
    {
        #region Private members

        private readonly IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ITokenServiceApiClient _tokenService;
        private readonly ILogger<HmrcService> _logger;
        private PrivilegedAccessToken _cachedToken;

        private const string AzureResource = "https://database.windows.net/"; // TODO: move to config
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        #endregion Private members

        #region Constructors
        public HmrcService(
            ITokenServiceApiClient tokenService,
            IApprenticeshipLevyApiClient apprenticeshipLevyService,
            ILogger<HmrcService> logger,
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider
            )
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _connectionString = applicationSettings.DbConnectionString;
            _cachedToken = null;
        }
        #endregion Constructors

        #region IsNationalInsuranceNumberRelatedToPayeScheme
        public async Task<EmploymentCheckCacheRequest> IsNationalInsuranceNumberRelatedToPayeScheme(
            EmploymentCheckCacheRequest request)
        {
            var thisMethodName = $"{nameof(HmrcService)}.IsNationalInsuranceNumberRelatedToPayeScheme";

            try
            {
                if (_cachedToken == null) await RetrieveAuthenticationToken();

                var policy = Policy
                    .Handle<UnauthorizedAccessException>()
                    .RetryAsync(
                        retryCount: 1,
                        onRetryAsync: async (outcome, retryNumber, context) => await RetrieveAuthenticationToken());

                var result = await policy.ExecuteAsync(() => GetEmploymentStatus(request));

                if (result != null)
                {
                    request.Employed = result.Employed;
                    request.RequestCompletionStatus = 200;

                    await StoreHmrcResponse(new EmploymentCheckCacheResponse(
                        request.ApprenticeEmploymentCheckId,
                        request.Id,
                        request.CorrelationId,
                        request.Employed,
                        request.PayeScheme,
                        true,
                        1,
                        "(OK)",
                        200));
                }
                else
                {
                    _logger.LogError($"{thisMethodName}: The result value returned from the GetEmploymentStatus call returned null.");

                    request.Employed = null;
                    request.RequestCompletionStatus = 500;
                }
            }
            catch (ApiHttpException e) when (e.HttpCode == (int) HttpStatusCode.NotFound)
            {
                _logger.LogInformation($"HMRC API returned {e.HttpCode} (Not Found)");

                // Note: We don't have access to the actual API response (it's within the levy service GetEmploymentStatus() call) so the response is populated with the description related to the HttpStatusCode
                await StoreHmrcResponse(new EmploymentCheckCacheResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    null,                   // Employed
                    string.Empty,           // FoundOnPaye
                    true,                   // ProcessingComplete
                    1,                      // Count
                    e.ResponseBody,         // Response
                    (short)e.HttpCode));
            }
            catch (ApiHttpException e) when (e.HttpCode == (int) HttpStatusCode.TooManyRequests)
            {
                _logger.LogError($"HMRC API returned {e.HttpCode} (Too Many Requests)");

                await StoreHmrcResponse(new EmploymentCheckCacheResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    null,                   // Employed
                    string.Empty,           // FoundOnPaye
                    false,                  // ProcessingComplete
                    1,                      // Count
                    e.ResponseBody,         // Response
                    (short)e.HttpCode));
            }
            catch (ApiHttpException e) when (e.HttpCode == (int) HttpStatusCode.BadRequest)
            {
                _logger.LogError("HMRC API returned {e.HttpCode} (Bad Request)");

                await StoreHmrcResponse(new EmploymentCheckCacheResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    null,                   // Employed
                    string.Empty,           // FoundOnPaye
                    false,                  // ProcessingComplete
                    1,                      // Count
                    e.ResponseBody,         // Response
                    (short)e.HttpCode));
            }

            catch (ApiHttpException e)
            {
                _logger.LogError($"HMRC API unhandled exception: {e.HttpCode} {e.Message}");

                await StoreHmrcResponse(new EmploymentCheckCacheResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    null,                   // Employed
                    string.Empty,           // FoundOnPaye
                    false,                  // ProcessingComplete
                    1,                      // Count
                    e.ResponseBody,         // Response
                    (short)e.HttpCode));
            }
            catch (Exception e)
            {
                _logger.LogError($"HMRC API unhandled exception: {e.Message} {e.StackTrace}");

                await StoreHmrcResponse(new EmploymentCheckCacheResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    null,                   // Employed
                    string.Empty,           // FoundOnPaye
                    false,                  // ProcessingComplete
                    1,                      // Count
                    $"HMRC API CALL ERROR {e.Message}",
                    500));
            }

            return request;

        }
        #endregion IsNationalInsuranceNumberRelatedToPayeScheme

        #region GetEmploymentStatus

        private async Task<EmploymentStatus> GetEmploymentStatus(EmploymentCheckCacheRequest request)
        {
            var employmentStatus = await _apprenticeshipLevyService.GetEmploymentStatus(
                _cachedToken.AccessCode,
                request.PayeScheme,
                request.Nino,
                request.MinDate,
                request.MaxDate
            );

            return employmentStatus;
        }

        #endregion GetEmploymentStatus

        #region RetrieveAuthenticationToken

        private async Task RetrieveAuthenticationToken()
        {
            _cachedToken = await _tokenService.GetPrivilegedAccessTokenAsync();
        }

        #endregion RetrieveAuthenticationToken

        #region StoreHmrcResponse

        public async Task<int> StoreHmrcResponse(
            EmploymentCheckCacheResponse employmentCheckCacheResponse)
        {
            if (employmentCheckCacheResponse == null) return await Task.FromResult(0);

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                AzureResource,
                _azureServiceTokenProvider))
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));

                await sqlConnection.OpenAsync();
                var parameter = new DynamicParameters();
                parameter.Add("@apprenticeEmploymentCheckId", employmentCheckCacheResponse.ApprenticeEmploymentCheckId, DbType.Int64);
                parameter.Add("@employmentCheckCacheRequestId", employmentCheckCacheResponse.EmploymentCheckCacheRequestId, DbType.Int64);
                parameter.Add("@correlationId", employmentCheckCacheResponse.CorrelationId, DbType.Guid);
                parameter.Add("@employed", employmentCheckCacheResponse.Employed, DbType.Boolean);
                parameter.Add("@foundOnPaye", employmentCheckCacheResponse.FoundOnPaye, DbType.String);
                parameter.Add("@processingComplete", employmentCheckCacheResponse.ProcessingComplete, DbType.Boolean);
                parameter.Add("@count", employmentCheckCacheResponse.Count, DbType.Int32);
                parameter.Add("@httpResponse", employmentCheckCacheResponse.HttpResponse, DbType.String);
                parameter.Add("@httpStatusCode", employmentCheckCacheResponse.HttpStatusCode, DbType.Int16);
                parameter.Add("@createdOn", DateTime.Now, DbType.DateTime);

                await sqlConnection.ExecuteScalarAsync(
                    "INSERT [Cache].[EmploymentCheckCacheResponse] " +
                    "       ( ApprenticeEmploymentCheckId,  EmploymentCheckCacheRequestId,  CorrelationId,  Employed,  FoundOnPaye,  ProcessingComplete, count,   httpResponse,  HttpStatusCode,  CreatedOn) " +
                    "VALUES (@apprenticeEmploymentCheckId, @EmploymentCheckCacheRequestId, @correlationId, @employed, @foundOnPaye, @processingComplete, @count, @httpResponse, @httpStatusCode, @createdOn)",
                    parameter,
                    commandType: CommandType.Text);
            }

            return await Task.FromResult(0);
        }

        #endregion StoreHmrcResponse
    }
}