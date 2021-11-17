using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public class HmrcService2
    {
        private IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private ITokenServiceApiClient _tokenService;

        public HmrcService2()
        {

        }

        public async Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme()
        {
            _apprenticeshipLevyService = new ApprenticeshipLevyApiClient(new HttpClient()
            {
                BaseAddress = new Uri("https://test-api.service.hmrc.gov.uk/")
            });

            var accessCode = "***************";
            var nino = "***********";
            var empRef = "***********";
            var fromDate = new DateTime(2010, 1, 1);
            var toDate = new DateTime(2018, 1, 1);

            var response = await _apprenticeshipLevyService.GetEmploymentStatus(accessCode, empRef,
                nino, fromDate, toDate);

            return response.Employed;
        }

        public async Task BombIt(int noOfCalls)
        {

            double delayInMs = 1000;

            for (var i = 0; i < noOfCalls; i += 10)
            {
                try
                {
                    var calls = new List<Task>();
                    for (int j = 0; j < 3; j++)
                    {
                        calls.Add(MakeTheCall(i));
                    }

                    await Task.WhenAll(calls);

                    Thread.Sleep(TimeSpan.FromMilliseconds(delayInMs));
                }
                catch (ApiHttpException e)
                {
                    throw new ApiHttpException(e.HttpCode, $"[{i}]" + e, "", "");
                }

            }
        }


        private async Task MakeTheCall(int number)
        {
            _apprenticeshipLevyService = new ApprenticeshipLevyApiClient(new HttpClient()
            {
                BaseAddress = new Uri("https://test-api.service.hmrc.gov.uk/")
            });

            var accessCode = "*********";
            var nino = "**********";
            var empRef = "*************";
            var fromDate = new DateTime(2010, 1, 1);
            var toDate = new DateTime(2018, 1, 1);

            try
            {
                await _apprenticeshipLevyService.GetEmploymentStatus(accessCode, empRef, nino, fromDate, toDate);
            }
            catch (ApiHttpException e)
            {
                throw new ApiHttpException(e.HttpCode, $"[{number}]" + e, "", "");
            }
        }
    }
}
