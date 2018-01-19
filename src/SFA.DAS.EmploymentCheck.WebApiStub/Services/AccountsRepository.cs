using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.WebApiStub.Services
{
    public class AccountsRepository : IAccountsRepository
    {

        private IDictionary<int, List<string>> _accounts;

        public AccountsRepository()
        {
            _accounts = new Dictionary<int, List<string>>();
        }

        public void AddAccounts(int id, List<string> value)
        {
            _accounts.Add(id, value);
        }

        public void DeleteAccount(int id)
        {
            _accounts.Remove(id);
        }

        public IEnumerable<List<string>> GetAllAccountsPaye()
        {
            return _accounts.Values;
        }

        public List<string> GetAnAccountsPaye(int id)
        {
            var result = _accounts.TryGetValue(id, out var value);

            return result ? value : new List<string>();
        }

        public void ModifyAccount(int id, List<string> value)
        {
            _accounts.Remove(id);
            _accounts.Add(id, value);
        }
    }
}
