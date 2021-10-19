using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Services.Stubs
{
    public class LearnerServiceStub : ILearnersService
    {
        private readonly ILogger<ILearnersService> _logger;

        public LearnerServiceStub(
            ILogger<ILearnersService> logger)
        {
            _logger = logger;
        }

        public async Task<LearnerNationalnsuranceNumberDto[]> GetLearnersNationalInsuranceNumbers(LearnerNationalnsuranceNumberDto[] learnersNinosDto)
        {
            var thisMethodName = "***** AccountsService.GetLearnersNationalInsuranceNumbers(LearnerNinoDto[] learnersNinosDto) *****";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            if (learnersNinosDto != null && learnersNinosDto.Length > 0)
            {
                for (int i = 0; i < learnersNinosDto.Length; i++)
                {
                    learnersNinosDto[i].NationalInsuranceNumber = "123456768";
                }
            }

            return await Task.FromResult(learnersNinosDto);
        }
    }
}



