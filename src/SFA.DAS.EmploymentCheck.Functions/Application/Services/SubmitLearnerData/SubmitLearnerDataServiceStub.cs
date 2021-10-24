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
        private readonly ILogger<ISubmitLearnerDataService> _logger;

        public SubmitLearnerDataServiceStub(
            ILogger<ISubmitLearnerDataService> logger)
        {
            _logger = logger;
        }

        public async Task<IList<ApprenticeNiNumber>> GetApprenticesNiNumber(IList<Apprentice> apprentices)
        {
            List<ApprenticeNiNumber> apprenticesNiNumber = new List<ApprenticeNiNumber>();
            if (apprentices != null && apprentices.Count > 0)
            {
                foreach(var apprentice in apprentices)
                {
                    var apprenticeNiNumber = new ApprenticeNiNumber(apprentice.ULN, "123456768");
                    apprenticesNiNumber.Add(apprenticeNiNumber);
                }
            }

            return await Task.FromResult(apprenticesNiNumber);
        }
    }
}



