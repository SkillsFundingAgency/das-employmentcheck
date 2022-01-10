﻿using SFA.DAS.EmploymentCheck.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Interfaces.LearnerData

{
    public interface ILearnerDataClient
    {
        Task<IList<LearnerNiNumber>> GetNiNumbers(IList<Domain.Entities.EmploymentCheck> apprentices);
    }
}
