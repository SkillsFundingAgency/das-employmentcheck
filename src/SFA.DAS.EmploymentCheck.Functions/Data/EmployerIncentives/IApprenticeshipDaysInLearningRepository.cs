﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives
{
    public interface IApprenticeshipDaysInLearningRepository
    {
        Task<IEnumerable<ApprenticeshipDaysInLearning>> GetAll();
    }
}