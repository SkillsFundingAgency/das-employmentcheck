using System.Threading.Tasks;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount
{
    public interface IEmployerAccountService
    {
        Task<ResourceList> GetPayeSchemes(Models.EmploymentCheck apprenticeEmploymentCheck);

        Task<int> StoreAccountsResponse(AccountsResponse accountsResponse);
    }
}
