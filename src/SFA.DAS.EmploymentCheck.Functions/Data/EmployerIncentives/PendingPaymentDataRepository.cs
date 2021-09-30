using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives
{
    public class PendingPaymentDataRepository : IPendingPaymentDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazycontext;
        private EmployerIncentivesDbContext _dbContext => _lazycontext.Value;

        public PendingPaymentDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazycontext = dbContext;
        }

        public async Task<IEnumerable<PendingPayment>> GetAll()
        {
            var pendingPayments = await _dbContext.Set<PendingPayment>().ToListAsync();
            if (pendingPayments != null && pendingPayments.Count > 0)
            {
                return pendingPayments;
            }
            return null;
        }
    }
}
