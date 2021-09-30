using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives
{
    public class PaymentDataRepository : IPaymentDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazycontext;
        private EmployerIncentivesDbContext _dbContext => _lazycontext.Value;

        public PaymentDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazycontext = dbContext;
        }

        public async Task<IEnumerable<Payment>> GetAll()
        {
            var payments = await _dbContext.Set<Payment>().ToListAsync();
            if (payments != null && payments.Count > 0)
            {
                return payments;
            }
            return null;
        }
    }
}
