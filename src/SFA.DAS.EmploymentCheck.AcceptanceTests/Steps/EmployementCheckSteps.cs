using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using BoDi;
using TechTalk.SpecFlow;
using Newtonsoft.Json;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    public class EmployementCheckSteps
    {
        private IObjectContainer _objectContainer;
        public EmployementCheckSteps(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [Given(@"An Account with an Account Id (.*) and EmpRef (.*) exists")]
        public async Task GivenAnAccountWithAnAccountIdAndEmpRefExists(string accountId, string empRef)
        {
            var accountclient = _objectContainer.Resolve<HttpClient>();

            var values = new KeyValuePair<string, List<string>>(accountId, empRef.Split(",").Select(x => x).ToList());

            var stringContent = new StringContent(JsonConvert.SerializeObject(values), Encoding.UTF8, "application/json");

            await accountclient.PostAsync("api/accounts", stringContent);

            var response = await accountclient.GetAsync("api/accounts");

            var result = response.Content.ReadAsStringAsync();
        }
        
        [Given(@"a call to the HMRC API with EmpRef (.*) and NINO (.*) response (.*)")]
        public void GivenACallToTheHMRCAPIWithEmpRefAndNINOResponse(string empRef, string nino, string hmrcresponse)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"A Submission Event has raised with EmpRef (.*) and NINO (.*) and ULN (.*)")]
        public void WhenASubmissionEventHasRaisedWithEmpRefAndNINOAndULN(string empRef, string nino, string uln)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I should have PassedValidationCheck (.*) for ULN (.*) and NINO (.*)")]
        public void ThenIShouldHavePassedValidationCheckForULNAndNINO(string check, string uln, string nino)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
