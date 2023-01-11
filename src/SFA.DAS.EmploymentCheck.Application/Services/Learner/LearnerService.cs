using SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.Learner
{
    public class LearnerService : ILearnerService
    {
        private readonly IDataCollectionsResponseRepository _repository;
        private readonly INationalInsuranceNumberService _nationalInsuranceNumberService;        

        public LearnerService(            
            IDataCollectionsResponseRepository repository,
            INationalInsuranceNumberService nationalInsuranceNumberService
        )
        {
            _repository = repository;
            _nationalInsuranceNumberService = nationalInsuranceNumberService;
        }

        public async Task<LearnerNiNumber> GetDbNiNumber(Data.Models.EmploymentCheck employmentCheck)
        {
            var response = await _repository.GetByEmploymentCheckId(employmentCheck.Id);
            if (response != null && response.NiNumber != null)
            {
                return new LearnerNiNumber(employmentCheck.Uln, response.NiNumber, HttpStatusCode.OK);
            }

            return null;
        }

        public Task<LearnerNiNumber> GetNiNumber(Data.Models.EmploymentCheck employmentCheck)
        {
            return _nationalInsuranceNumberService.Get(new NationalInsuranceNumberRequest(employmentCheck));
        }                                 
    }
}