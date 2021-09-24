using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives
{
    public class ApprenticeshipIncentiveDataRepository : IApprenticeshipIncentiveDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazycontext;
        private EmployerIncentivesDbContext _dbContext => _lazycontext.Value;

        public ApprenticeshipIncentiveDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazycontext = dbContext;
        }

        public async Task<IEnumerable<ApprenticeshipIncentive>> GetAll()
        {
            var apprenticeshipIncentives = await _dbContext.Set<ApprenticeshipIncentive>().ToListAsync();
            if (apprenticeshipIncentives != null && apprenticeshipIncentives.Count > 0)
            {
                return apprenticeshipIncentives;
            }
            return null;
        }
    }
}
