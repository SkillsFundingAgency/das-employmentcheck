using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.SubmitLearnerData
{
    public class SubmitLearnerDataService : ISubmitLearnerDataService
    {
        private readonly ILogger<SubmitLearnerDataService> _logger;
        private readonly IDcTokenService _dcTokenService;
        private readonly IHttpClientFactory _httpFactory;
        private readonly DcApiSettings _dcApiSettings;

        public SubmitLearnerDataService(
            ILogger<SubmitLearnerDataService> logger,
            IDcTokenService dcTokenService, 
            IHttpClientFactory httpFactory,
            IOptions<DcApiSettings> dcApiSettings)
        {
            _logger = logger;
            _dcTokenService = dcTokenService;
            _httpFactory = httpFactory;
            _dcApiSettings = dcApiSettings.Value;
        }

        public async Task<IList<ApprenticeNiNumber>> GetApprenticesNiNumber(IList<ApprenticeEmploymentCheckModel> apprentices)
        {
            var thisMethodName = $"{nameof(SubmitLearnerDataService)}.GetApprenticeNiNumbers()";

            IList<ApprenticeNiNumber> apprenticeNiNumbers = new List<ApprenticeNiNumber>();
            IList<ApprenticeEmploymentCheckModel> apprenticesToCheck = new List<ApprenticeEmploymentCheckModel>();

            try
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Checking for NINOs present already in database");
                foreach (var apprentice in apprentices)
                {
                    if (!string.IsNullOrEmpty(apprentice.NationalInsuranceNumber))
                    {
                        apprenticeNiNumbers.Add(new ApprenticeNiNumber(apprentice.ULN, apprentice.NationalInsuranceNumber));
                    }
                    else
                    {
                        apprenticesToCheck.Add(apprentice);
                    }
                }
                _logger.LogInformation($"\n\n{thisMethodName}: {apprenticeNiNumbers.Count} NINOs already in database, not calling DC API for those records");
                if (apprenticesToCheck.Count > 0)
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: Calling DC API for {apprenticesToCheck.Count} record(s)");
                    var token = await GetDcToken();
                    var returnedApprenticeNiNumbers = await GetNiNumbers(apprenticesToCheck, token);
                    foreach (var apprenticeNiNumber in returnedApprenticeNiNumbers)
                    {
                        apprenticeNiNumbers.Add(apprenticeNiNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(apprenticeNiNumbers);
        }

        private async Task<AuthResult> GetDcToken()
        {
            var thisMethodName = $"{nameof(SubmitLearnerDataService)}.GetDcToken()";

            var result = new AuthResult();
            try
            {
                result = await _dcTokenService.GetTokenAsync(
                    $"https://login.microsoftonline.com/{_dcApiSettings.Tenant}",
                    "client_credentials",
                    _dcApiSettings.ClientSecret,
                    _dcApiSettings.ClientId,
                    _dcApiSettings.IdentifierUri);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }
            return result;
        }

        private async Task<ApprenticeNiNumber> SendIndividualRequest(ApprenticeEmploymentCheckModel learner, AuthResult token)
        {
            var thisMethodName = $"{nameof(SubmitLearnerDataService)}.SendIndividualRequest()";

            var checkedLearner = new ApprenticeNiNumber();
            using (var client = _httpFactory.CreateClient("LearnerNiApi"))
            {
                client.BaseAddress = new Uri(_dcApiSettings.BaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                var url = $"/api/v1/ilr-data/learnersNi/2122?ulns={learner.ULN}";

                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = await response.Content.ReadAsStreamAsync();
                        if (result.Length > 0)
                        {
                            var checkedLearners = await JsonSerializer.DeserializeAsync<List<ApprenticeNiNumber>>(result);
                            checkedLearner = checkedLearners.FirstOrDefault();
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"\n\n{thisMethodName}: response code received from LearnerNiApi is {response.StatusCode}");
                        checkedLearner.ULN = learner.ULN;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                }
            }
            return checkedLearner;
        }

        private async Task<IList<ApprenticeNiNumber>> GetNiNumbers(ICollection<ApprenticeEmploymentCheckModel> learners, AuthResult token)
        {
            var thisMethodName = $"{nameof(SubmitLearnerDataService)}.GetNiNumbers()";

            _logger.LogInformation($"{thisMethodName}: Getting Ni Numbers for {learners.Count} apprentices");

            var timer = new Stopwatch();
            timer.Start();
            var checkedLearners = new List<ApprenticeNiNumber>();

            var taskList = new List<Task<ApprenticeNiNumber>>();

            foreach (var learner in learners)
            {
                taskList.Add(SendIndividualRequest(learner, token));

                var responses = await Task.WhenAll(taskList);
                checkedLearners.AddRange(responses);

                taskList.Clear();
            }

            timer.Stop();
            _logger.LogInformation($"{thisMethodName}: Got Ni Numbers for {learners.Count} apprentices. {timer}ms elapsed");

            return checkedLearners;
        }
    }
}