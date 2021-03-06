﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using TechTalk.SpecFlow;
using BoDi;
using SFA.DAS.ProviderEventsApiSubstitute.WebAPI;
using SFA.DAS.AccountsApiSubstitute.WebAPI;
using SFA.DAS.ApiSubstitute.Utilities;
using SFA.DAS.EmploymentCheck.SubmissionEventWorkerRole;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.HmrcApiSubstitute.WebAPI;
using HMRC.ESFA.Levy.Api.Types;
using SFA.DAS.Provider.Events.Api.Types;
using SFA.DAS.CommitmentsApiSubstitute.WebAPI;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Infrastructure;
using Polly;
using SFA.DAS.EmploymentCheck.Domain.Models;
using NUnit.Framework;
using System.Net;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    public class ComplianceSteps
    {
        private readonly List<TimeSpan> _sleepDurations = new List<TimeSpan>()
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(8)
        };

        private IObjectContainer _objectContainer;
        private IObjectCreator _objectCreator;
        private EmploymentCheckRepository _repository;
        private long _uln;
        private List<PreviousHandledSubmissionEvent> _results;

        public ComplianceSteps(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
            _objectCreator = new ObjectCreator();
            _repository = _objectContainer.Resolve<EmploymentCheckRepository>();
        }

        [Given(@"A Submission Event has raised with Apprenticeship (.*) and NINO (.*) and ULN (.*) and Ukprn (.*)")]
        public void WhenASubmissionEventHasRaisedWithEmpRefAAAndNINOQQCAndULN(string apprenticeshipId, string nino, string uln, string ukprn)
        {
            _uln = long.Parse(uln);

            var providereventsApiMessageHandlers = _objectContainer.Resolve<ProviderEventsApiMessageHandler>();

            var submissionEvents = _objectCreator.Create<SubmissionEvent>(x => { x.ActualStartDate = new DateTime(2017, 12, 14); x.ApprenticeshipId = long.Parse(apprenticeshipId); x.Ukprn = long.Parse(ukprn); x.NiNumber = nino; x.Id = 1; x.Uln = _uln; });

            providereventsApiMessageHandlers.SetupGetSubmissionEvents(1, new PageOfResults<SubmissionEvent> { Items = new[] { submissionEvents }, PageNumber = 1, TotalNumberOfPages = 1 });
        }

        [Given(@"a Commitment with Apprenticeship (.*) and Ukprn (.*) and Account Id (.*) throws 401")]
        public void GivenACommitmentWithApprenticeshipAndUkprnAndAccountIdThrows(string apprenticeshipId, string ukprn, string accountid)
        {
            var commitmentsApiMessageHandlers = _objectContainer.Resolve<CommitmentsApiMessageHandler>();

            var apprenticeship = _objectCreator.Create<Apprenticeship>(x => { x.ProviderId = long.Parse(ukprn); x.Id = long.Parse(apprenticeshipId); x.EmployerAccountId = long.Parse(accountid); });

            commitmentsApiMessageHandlers.SetupGetProviderApprenticeship(long.Parse(ukprn), long.Parse(apprenticeshipId), apprenticeship,System.Net.HttpStatusCode.Unauthorized);
        }


        [Given(@"a Commitment with Apprenticeship (.*) and Ukprn (.*) and Account Id (.*) exists")]
        public void GivenACommitmentWithApprenticeshipAndUkprnAndAccountIdExists(string apprenticeshipId, string ukprn, string accountid)
        {
            var commitmentsApiMessageHandlers = _objectContainer.Resolve<CommitmentsApiMessageHandler>();

            var apprenticeship = _objectCreator.Create<Apprenticeship>(x => { x.ProviderId = long.Parse(ukprn); x.Id = long.Parse(apprenticeshipId); x.EmployerAccountId = long.Parse(accountid); });

            commitmentsApiMessageHandlers.SetupGetProviderApprenticeship(long.Parse(ukprn), long.Parse(apprenticeshipId), apprenticeship);
        }


        [Given(@"An Account with an Account Id (.*) and EmpRef (.*) exists")]
        public void GivenAnAccountWithAnAccountIdAndEmpRefAAExists(string accountid, string payeschemes)
        {
            if (ScenarioContext.Current.ScenarioInfo.Tags.Contains("addmoreEmpRefstoanAccount"))
            {
                List<string> extrapaye = new List<string>();
                for (int i = 0000; i <= 9996; i++)
                {
                    var firstpart = i.ToString().Length == 1 ? $"000{i.ToString()}" : i.ToString().Length == 2 ? $"00{i.ToString()}" : i.ToString().Length == 3 ? $"0{i.ToString()}" : i.ToString();
                    extrapaye.Add($"{firstpart}/AA00001");
                }
                payeschemes = string.Join(",", extrapaye) + $",{payeschemes}";

                _sleepDurations.AddRange(new List<TimeSpan>(){TimeSpan.FromMinutes(3),TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(8), TimeSpan.FromMinutes(10)});
            }

            AddAccountDetails(accountid, payeschemes);
        }
        
        private void AddAccountDetails(string accountid, string payeschemes)
        {
            var accountsApiMessageHandlers = _objectContainer.Resolve<AccountsApiMessageHandler>();
            
            var resourceList = new ResourceList(EmpRefs(accountid, payeschemes));

            var accountmodel = _objectCreator.Create<AccountDetailViewModel>(x => { x.AccountId = long.Parse(accountid); x.PayeSchemes = resourceList; });

            accountsApiMessageHandlers.SetupGetAccount(long.Parse(accountid), accountmodel);
            accountsApiMessageHandlers.SetupCall($"/api/accounts/internal/{accountid}", System.Net.HttpStatusCode.OK, accountmodel);
        }


        [Given(@"Hmrc Api is configured as")]
        public void GivenHmrcApiIsConfiguredAs(Table table)
        {
            ConfigureHmrcApi(table, HttpStatusCode.OK);
        }

        [Given(@"Hmrc Api is configured to return Not Found for")]
        public void GivenHmrcApiIsConfiguredToReturnNotFoundFor(Table table)
        {
            ConfigureHmrcApi(table, HttpStatusCode.NotFound);
        }

        private void ConfigureHmrcApi(Table table, HttpStatusCode httpStatusCode)
        {
            var hmrcApiMessageHandlers = _objectContainer.Resolve<HmrcApiMessageHandler>();

            foreach (var row in table.Rows)
            {
                var hmrcresponse = row["Response"];
                var paye = row["Paye"];
                var nino = row["Nino"];

                var stubresponse = _objectCreator.Create<EmploymentStatus>(x => { x.Employed = hmrcresponse == "Employed" ? true : false; x.Empref = paye; x.Nino = nino; x.FromDate = new DateTime(2017, 12, 14); x.ToDate = DateTime.Now; });

                hmrcApiMessageHandlers.SetupGetEmploymentStatus(stubresponse, paye, nino, new DateTime(2017, 12, 14), DateTime.Now, httpStatusCode);
            }
        }

        [When(@"I run the worker role")]
        public async Task WhenIRunTheWorkerRole()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var sut = _objectContainer.Resolve<WorkerRole>();
            Task.Run(() => sut.Run(), cancellationToken);

            await Policy
                    .HandleResult<long>(r => r != 1)
                    .WaitAndRetryAsync(_sleepDurations)
                    .ExecuteAsync(async () => await _repository.GetLastProcessedEventId());
            
            
            _results = await Policy
                    .HandleResult<List<PreviousHandledSubmissionEvent>>(r => r.Count != 1)
                    .WaitAndRetryAsync(_sleepDurations)
                    .ExecuteAsync(async () => await _repository.GetPreviouslyHandledSubmissionEvents(_uln));

            cancellationTokenSource.Cancel();

        }

        [Then(@"I should have PassedValidationCheck (Yes|No) for ULN (.*) and NINO (.*)")]
        public void ThenIShouldHavePassedValidationCheckYesForULNAndNINOQQC(string validationCheck, string uln, string nino)
        {
            Assert.AreEqual(validationCheck == "Yes" ? true : false, _results.FirstOrDefault(x=> x.Uln == long.Parse(uln))?.PassedValidationCheck);
        }

        private List<ResourceViewModel> EmpRefs(string accountid, string empRefs)
        {
            var payeschemes = new List<ResourceViewModel>();

            foreach (var empref in empRefs.Split(','))
            {
                payeschemes.Add(new ResourceViewModel { Id = empref, Href = $"api/accounts/{accountid}/payescheme/{empref}" });
            }

            return payeschemes;
        }
    }
}
