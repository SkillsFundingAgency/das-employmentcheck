using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives
{
    public class ApprenticeshipDaysInLearningRepository : IApprenticeshipDaysInLearningRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazycontext;
        private EmployerIncentivesDbContext _dbContext => _lazycontext.Value;

        public ApprenticeshipDaysInLearningRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazycontext = dbContext;
        }

        public async Task<IEnumerable<ApprenticeshipDaysInLearning>> GetAll()
        {
            var daysInLearnings = await _dbContext.Set<ApprenticeshipDaysInLearning>().ToListAsync();
            if (daysInLearnings != null && daysInLearnings.Count > 0)
            {
                return daysInLearnings;
            }
            return null;
        }
    }
}
