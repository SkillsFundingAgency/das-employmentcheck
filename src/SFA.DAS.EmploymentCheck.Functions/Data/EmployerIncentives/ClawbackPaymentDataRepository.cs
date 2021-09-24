using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives
{
    public class ClawbackPaymentDataRepository : IClawbackPaymentDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazycontext;
        private EmployerIncentivesDbContext _dbContext => _lazycontext.Value;

        public ClawbackPaymentDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazycontext = dbContext;
        }

        public async Task<IEnumerable<ClawbackPayment>> GetAll()
        {
            var clawbackPayments = await _dbContext.Set<ClawbackPayment>().ToListAsync();
            if (clawbackPayments != null && clawbackPayments.Count > 0)
            {
                return clawbackPayments;
            }
            return null;
        }
    }
}
