using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives
{
    public class PendingPaymentValidationResultDataRepository : IPendingPaymentValidationResultDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazycontext;
        private EmployerIncentivesDbContext _dbContext => _lazycontext.Value;

        public PendingPaymentValidationResultDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazycontext = dbContext;
        }

        public async Task<IEnumerable<PendingPaymentValidationResult>> GetAll()
        {
            var pendingPaymentValidationResults = await _dbContext.Set<PendingPaymentValidationResult>().ToListAsync();
            if (pendingPaymentValidationResults != null && pendingPaymentValidationResults.Count > 0)
            {
                return pendingPaymentValidationResults;
            }
            return null;
        }
    }
}
