using System;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmploymentCheck.UserAcceptanceTests
{
    [Binding]
    public class ComplianceSteps
    {
        [Given(@"An Account with an Account Id (.*) and EmpRef (.*) exists")]
        public void GivenAnAccountWithAnAccountIdAndEmpRefAAExists(int p0, int p1, int p2)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"a call to the HMRC API with EmpRef (.*) and NINO (.*) response (Employed|NotEmployed)")]
        public void GivenACallToTheHMRCAPIWithEmpRefAAAndNINOQQCResponseEmployed(int p0, int p1, int p2)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"A Submission Event has raised with EmpRef (.*) and NINO (.*) and ULN (.*)")]
        public void WhenASubmissionEventHasRaisedWithEmpRefAAAndNINOQQCAndULN(int p0, int p1, int p2, Decimal p3)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I should have PassedValidationCheck (Yes|No) for ULN (.*) and NINO (.*)")]
        public void ThenIShouldHavePassedValidationCheckYesForULNAndNINOQQC(Decimal p0, int p1)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
