using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Interfaces.EmployerAccount
{
    public interface IEmployerAccountService
    {
        Task<ResourceList> GetPayeSchemes(Domain.Entities.EmploymentCheck apprenticeEmploymentCheck);

        Task<int> StoreAccountsResponse(AccountsResponse accountsResponse);
    }
}
