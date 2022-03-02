using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IAccountsResponseRepository
    {
        Task InsertOrUpdate(AccountsResponse response);

        Task Save(AccountsResponse response);
    }
}
