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
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly ILogger<ISubmitLearnerDataService> _logger;

        public SubmitLearnerDataServiceStub(
            ILogger<ISubmitLearnerDataService> logger)
        {
            _logger = logger;
        }

        public async Task<IList<ApprenticeNiNumber>> GetApprenticesNiNumber(IList<EmploymentCheckModel> employmentCheckModels)
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticesNiNumber()";

            IList<ApprenticeNiNumber> apprenticesNiNumber = new List<ApprenticeNiNumber>();
            if (employmentCheckModels != null &&
                employmentCheckModels.Count > 0)
            {
                foreach(var employmentCheckModel in employmentCheckModels)
                {
                    var apprenticeNiNumber = await FindApprenticeNiNumber(employmentCheckModel);
                    apprenticesNiNumber.Add(apprenticeNiNumber);
                }
            }

            _logger.LogInformation($"{thisMethodName}: returned {apprenticesNiNumber.Count} NI Numbers.");
            return await Task.FromResult(apprenticesNiNumber);
        }

        private async Task<ApprenticeNiNumber> FindApprenticeNiNumber(EmploymentCheckModel employmentCheckModel)
        {
            var uln = employmentCheckModel.Uln;
            var niNumber = "NI" + employmentCheckModel.Uln.ToString();

            var apprenticeNiNumber = new ApprenticeNiNumber
            {
                Uln = uln,
                NationalInsuranceNumber = niNumber
            };

            return await Task.FromResult(apprenticeNiNumber);
        }

        private async Task<ApprenticeNiNumber> FindApprenticeNiNumber2(Models.Domain.EmploymentCheckModel employmentCheckModel)
        {
            ApprenticeNiNumber apprenticeNiNumber;

            switch (employmentCheckModel.ApprenticeshipId)
            {
                case 1:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        Uln = 1000000001,
                        NationalInsuranceNumber = "NI1000000001"
                    };
                    break;

                case 2:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        Uln = 2000000002,
                        NationalInsuranceNumber = "NI2000000002"
                    };
                    break;

                case 3:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        Uln = 3000000003,
                        NationalInsuranceNumber = "NI3000000003"
                    };
                    break;

                case 4:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        Uln = 4000000004,
                        NationalInsuranceNumber = "NI4000000004"
                    };
                    break;

                case 5:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        Uln = 5000000005,
                        NationalInsuranceNumber = "NI5000000005"
                    };
                    break;

                case 6:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        Uln = 6000000006,
                        NationalInsuranceNumber = "NI6000000006"
                    };
                    break;

                case 7:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        Uln = 7000000007,
                        NationalInsuranceNumber = "NI7000000007"
                    };
                    break;

                case 8:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        Uln = 8000000008,
                        NationalInsuranceNumber = "NI8000000008"
                    };
                    break;

                case 9:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        Uln = 9000000009,
                        NationalInsuranceNumber = "NI9000000009"
                    };
                    break;

                default:
                    apprenticeNiNumber = new ApprenticeNiNumber
                    {
                        Uln = 1000000001,
                        NationalInsuranceNumber = "NI1000000001"
                    };
                    break;
            }

            return await Task.FromResult(apprenticeNiNumber);
        }
    }
}



