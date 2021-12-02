using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

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
            const string thisMethodName = "SubmitLearnerDataService.GetApprenticeNiNumbers()";

            IList<ApprenticeNiNumber> apprenticeNiNumbers = null;
            try
            {
                var token = GetDcToken().Result;

                apprenticeNiNumbers = GetNiNumbers(apprentices, token).Result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(apprenticeNiNumbers);
        }

        private async Task<AuthResult> GetDcToken()
        {
            var thisMethodName = "SubmitLearnerDataService.GetDcToken()";

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
            const string thisMethodName = "SubmitLearnerDataService.SendIndividualRequest()";

            var checkedLearner = new ApprenticeNiNumber();
            using (var client = _httpFactory.CreateClient("LearnerNiApi"))
            {
                client.BaseAddress = new Uri(_dcApiSettings.BaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                var url = "/api/v1/ilr-data/learnersNi/2021?ulns=" + learner.ULN;

                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = await response.Content.ReadAsStreamAsync();
                        if (result.Length > 0)
                        {
                            try
                            {
                                var checkedLearners = await JsonSerializer.DeserializeAsync<List<ApprenticeNiNumber>>(result);
                                checkedLearner = checkedLearners.FirstOrDefault();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(
                                    $"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                            }
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

        private async Task<List<ApprenticeNiNumber>> GetNiNumbers(IList<ApprenticeEmploymentCheckModel> learners, AuthResult token)
        {
            var thisMethodName = "SubmitLearnerDataService.GetNiNumbers()";

            _logger.LogInformation($"{thisMethodName}: Getting Ni Numbers for {learners.Count} apprentices");

            Stopwatch timer = new Stopwatch();
            timer.Start();
            List<ApprenticeNiNumber> checkedLearners = new List<ApprenticeNiNumber>();

            int counter = 0;
            var taskList = new List<Task<ApprenticeNiNumber>>();

            foreach (var learner in learners)
            {
                taskList.Add(SendIndividualRequest(learner, token));

                counter++;

                if (counter == 10)
                {
                    var response = await Task.WhenAll(taskList);
                    checkedLearners.AddRange(response);

                    taskList.Clear();
                    counter = 0;
                }
            }

            timer.Stop();
            _logger.LogInformation($"{thisMethodName}: Got Ni Numbers for {learners.Count} apprentices. {timer}ms elapsed");

            return checkedLearners;
        }
    }
}