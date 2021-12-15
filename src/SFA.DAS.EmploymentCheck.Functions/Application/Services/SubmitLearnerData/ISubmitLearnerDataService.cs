﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.SubmitLearnerData
{
    public interface ISubmitLearnerDataService
    {
        Task<IList<ApprenticeNiNumber>> GetApprenticesNiNumber(IList<EmploymentCheckModel> apprentices);

        //Task<IList<DataCollectionsApiResult>> GetApprenticesNiNumber(IList<EmploymentCheckModel> apprentices);
    }
}
