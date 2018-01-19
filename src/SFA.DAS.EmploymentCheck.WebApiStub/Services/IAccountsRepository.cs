using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.WebApiStub.Services
{
    public interface IAccountsRepository
    {
        IEnumerable<List<string>> GetAllAccountsPaye();

        List<string> GetAnAccountsPaye(int id);

        void AddAccounts(int id, List<string> value);

        void ModifyAccount(int id, List<string> value);

        void DeleteAccount(int id);
    }
}
