using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ApprenticeEmploymentCheck> ApprenticeEmploymentChecks { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}


