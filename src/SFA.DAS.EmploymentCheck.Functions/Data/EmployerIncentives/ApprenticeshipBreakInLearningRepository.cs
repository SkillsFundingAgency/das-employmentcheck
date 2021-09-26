using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives
{
    public class ApprenticeshipBreakInLearningRepository : IApprenticeshipBreakInLearningRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazycontext;
        private EmployerIncentivesDbContext _dbContext => _lazycontext.Value;

        public ApprenticeshipBreakInLearningRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazycontext = dbContext;
        }

        public async Task<IEnumerable<ApprenticeshipBreakInLearning>> GetAll()
        {
            var breakInLearnings = await _dbContext.Set<ApprenticeshipBreakInLearning>().ToListAsync();
            if (breakInLearnings != null && breakInLearnings.Count > 0)
            {
                return breakInLearnings;
            }
            return null;
        }
    }
}
