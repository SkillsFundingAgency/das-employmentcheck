using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmploymentCheck.WebApiStub.Services;

namespace SFA.DAS.EmploymentCheck.WebApiStub.Controllers
{

    [Route("api/[controller]")]
    public class AccountsController : Controller
    {
        private IAccountsRepository _accounts;

        public AccountsController(IAccountsRepository accounts)
        {
            _accounts = accounts;
        }

        // GET api/accounts
        [HttpGet]
        public IEnumerable<List<string>> Get()
        {
            return _accounts.GetAllAccountsPaye();
        }

        // GET api/accounts/24979
        [HttpGet("{id}")]
        public List<string> Get(int id)
        {
            return _accounts.GetAnAccountsPaye(id);
        }

        // POST api/accounts
        [HttpPost]
        public void Post([FromBody]KeyValuePair<int, List<string>> value)
        {
            _accounts.AddAccounts(value.Key, value.Value);
        }

        // PUT api/accounts/24979
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]List<string> value)
        {
            _accounts.ModifyAccount(id, value);
        }

        // DELETE api/accounts/
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _accounts.DeleteAccount(id);
        }
    }
}
