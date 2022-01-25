using SFA.DAS.EmploymentCheck.Data.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public interface IAccountsResponseRepository
    {
        Task Save(AccountsResponse accountsResponse);
    }
}
