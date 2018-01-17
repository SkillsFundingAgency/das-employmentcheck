using System;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    public class EmployementCheckSteps
    {
        [Given(@"An Account with an Account Id (.*) and EmpRef (.*) exists")]
        public void GivenAnAccountWithAnAccountIdAndEmpRefExists(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"a call to the HMRC API with EmpRef (.*) and NINO (.*) response (.*)")]
        public void GivenACallToTheHMRCAPIWithEmpRefAndNINOResponse(string p0, string p1, string p2)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"A Submission Event has raised with EmpRef (.*) and NINO (.*) and ULN (.*)")]
        public void WhenASubmissionEventHasRaisedWithEmpRefAndNINOAndULN(string p0, string p1, string p2)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I should have PassedValidationCheck (.*) for ULN (.*) and NINO (.*)")]
        public void ThenIShouldHavePassedValidationCheckForULNAndNINO(string p0, string p1, string p2)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
