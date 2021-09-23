using System.Threading.Tasks;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public interface IAccountsService
    {
        Task<AccountDetailViewModel> GetAccountDetail(string hashedAccountId);
    }
}
