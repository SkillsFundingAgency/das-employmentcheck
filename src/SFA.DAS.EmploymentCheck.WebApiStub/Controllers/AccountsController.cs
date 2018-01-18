using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.EmploymentCheck.WebApiStub.Controllers
{

    [Route("api/[controller]")]
    public class AccountsController : Controller
    {
        private Dictionary<int, List<string>> _accounts;

        public AccountsController()
        {
             _accounts = new Dictionary<int, List<string>>();
        }

        // GET api/accounts
        [HttpGet]
        public IEnumerable<List<string>> Get()
        {
            return _accounts.Values;
        }

        // GET api/accounts/24979
        [HttpGet("{id}")]
        public List<string> Get(int id)
        {
            return _accounts[id];
        }

        // POST api/accounts
        [HttpPost]
        public void Post([FromBody]KeyValuePair<int, List<string>> value)
        {
            _accounts.Add(value.Key, value.Value);
        }

        // PUT api/accounts/24979
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]List<string> value)
        {
            _accounts.Remove(id);
            _accounts.Add(id, value);
        }

        // DELETE api/accounts/
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _accounts.Remove(id);
        }
    }

    protected class AccountsRepository
    {
        private Dictionary<int, List<string>> _accountsRepo;


    }
}
