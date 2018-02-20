﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.2.0.0
//      SpecFlow Generator Version:2.2.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Features
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.2.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Compliance")]
    public partial class ComplianceFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Compliance.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Compliance", "\tIn order to identify learners \r\n\tAs a math idiot\r\n\tI want to be told the sum of " +
                    "two numbers", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Employment Check")]
        [NUnit.Framework.CategoryAttribute("AMl1297")]
        [NUnit.Framework.TestCaseAttribute("24979", "333/AA00001", "QQ123456C", "5641235789", "Employed", "Yes", "10007898", "112233", null)]
        [NUnit.Framework.TestCaseAttribute("24979", "333/AA00001", "QQ123456D", "5641235779", "NotEmployed", "No", "10007898", "112234", null)]
        public virtual void EmploymentCheck(string accountId, string empRef, string nino, string uln, string hmrcresponse, string check, string ukprn, string apprenticeshipId, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "AMl1297"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Employment Check", @__tags);
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
testRunner.Given(string.Format("A Submission Event has raised with Apprenticeship {0} and NINO {1} and ULN {2} an" +
                        "d Ukprn {3}", apprenticeshipId, nino, uln, ukprn), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 9
testRunner.And(string.Format("a Commitment with Apprenticeship {0} and Ukprn {1} and Account Id {2} exists", apprenticeshipId, ukprn, accountId), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 10
testRunner.And(string.Format("An Account with an Account Id {0} and EmpRef {1} exists", accountId, empRef), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
testRunner.And(string.Format("a call to the HMRC API with EmpRef {0} and NINO {1} response {2}", empRef, nino, hmrcresponse), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
testRunner.When("I run the worker role", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
testRunner.Then(string.Format("I should have PassedValidationCheck {0} for ULN {1} and NINO {2}", check, uln, nino), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
