using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.SubmitLearnerData;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.StubsSubmitLearnerData
{
    public class SubmitLearnerDataServiceStub : ISubmitLearnerDataService
    {
        private const string ThisClassName = "\n\nEmployerAccountServiceStub";
        private readonly ILogger<ISubmitLearnerDataService> _logger;

        public SubmitLearnerDataServiceStub(
            ILogger<ISubmitLearnerDataService> logger)
        {
            _logger = logger;
        }

        public async Task<IList<ApprenticeNiNumber>> GetApprenticesNiNumber(IList<EmploymentCheckModel> apprenticeEmploymentChecks)
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticesNiNumber()";

            List<ApprenticeNiNumber> apprenticesNiNumber = new List<ApprenticeNiNumber>();
            if (apprenticeEmploymentChecks != null &&
                apprenticeEmploymentChecks.Count > 0)
            {
                foreach(var apprenticeEmploymentCheck in apprenticeEmploymentChecks)
                {
                    var apprenticeNiNumber = await FindApprenticeNiNumber(apprenticeEmploymentCheck);
                    apprenticesNiNumber.Add(apprenticeNiNumber);
                }
            }

            _logger.LogInformation($"{thisMethodName}: returned {apprenticesNiNumber.Count} NI Numbers.");
            return await Task.FromResult(apprenticesNiNumber);
        }

        private async Task<ApprenticeNiNumber> FindApprenticeNiNumber(EmploymentCheckModel apprenticeEmploymentCheck)
        {
            var uln = apprenticeEmploymentCheck.Uln;
            var niNumber = "NI" + apprenticeEmploymentCheck.Uln.ToString();

            var apprenticeNiNumber = new ApprenticeNiNumber
            {
                ULN = uln,
                NationalInsuranceNumber = niNumber
            };

            return await Task.FromResult(apprenticeNiNumber);
        }

        private async Task<ApprenticeNiNumber> FindApprenticeNiNumber2(EmploymentCheckModel apprenticeEmploymentCheck)
        {
            ApprenticeNiNumber apprenticeNiNumber;

            switch (apprenticeEmploymentCheck.ApprenticeshipId)
            {
                case 1:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        ULN = 1000000001,
                        NationalInsuranceNumber = "NI1000000001"
                    };
                    break;

                case 2:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        ULN = 2000000002,
                        NationalInsuranceNumber = "NI2000000002"
                    };
                    break;

                case 3:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        ULN = 3000000003,
                        NationalInsuranceNumber = "NI3000000003"
                    };
                    break;

                case 4:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        ULN = 4000000004,
                        NationalInsuranceNumber = "NI4000000004"
                    };
                    break;

                case 5:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        ULN = 5000000005,
                        NationalInsuranceNumber = "NI5000000005"
                    };
                    break;

                case 6:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        ULN = 6000000006,
                        NationalInsuranceNumber = "NI6000000006"
                    };
                    break;

                case 7:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        ULN = 7000000007,
                        NationalInsuranceNumber = "NI7000000007"
                    };
                    break;

                case 8:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        ULN = 8000000008,
                        NationalInsuranceNumber = "NI8000000008"
                    };
                    break;

                case 9:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        ULN = 9000000009,
                        NationalInsuranceNumber = "NI9000000009"
                    };
                    break;

                default:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        ULN = 1000000001,
                        NationalInsuranceNumber = "NI1000000001"
                    };
                    break;
            }

            return await Task.FromResult(apprenticeNiNumber);
        }
    }
}



