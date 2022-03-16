using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IAccountsResponseRepository
    {
        Task InsertOrUpdate(AccountsResponse response);

        Task Save(AccountsResponse response);
    }
}
