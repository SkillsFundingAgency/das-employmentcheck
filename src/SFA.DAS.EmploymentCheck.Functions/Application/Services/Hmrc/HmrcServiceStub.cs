using HMRC.ESFA.Levy.Api.Client;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public class HmrcServiceStub : IHmrcService
    {
        private const string ThisClassName = "\n\nHmrcServiceStub";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly ILogger<IApprenticeshipLevyApiClient> _logger;

        public HmrcServiceStub(
            ILogger<IApprenticeshipLevyApiClient> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns the employment status for the given apprentice NationalInsuranceNumber and PayeScheme between the given Start Date and End Date
        /// </summary>
        /// <param name="apprenticeEmploymentCheckMessageModel"></param>
        /// <returns></returns>
        public async Task<ApprenticeEmploymentCheckMessageModel> IsNationalInsuranceNumberRelatedToPayeScheme(
            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            if(apprenticeEmploymentCheckMessageModel != null)
            {
                apprenticeEmploymentCheckMessageModel.IsEmployed = true;
            }

            return await Task.FromResult(apprenticeEmploymentCheckMessageModel);
        }
    }
}
