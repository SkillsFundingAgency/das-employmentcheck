using System.Threading.Tasks;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount
{
    public interface IEmployerAccountService
    {
        Task<AccountDetailViewModel> GetEmployerAccount(long accountId);
    }
}
