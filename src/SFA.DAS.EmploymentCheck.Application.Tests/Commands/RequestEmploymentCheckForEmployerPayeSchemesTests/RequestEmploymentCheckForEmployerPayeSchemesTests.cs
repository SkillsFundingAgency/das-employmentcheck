using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Application.Commands.RequestEmploymentCheckForEmployerPayeSchemes;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmploymentCheck.Application.Tests.Commands.RequestEmploymentCheckForEmployerPayeSchemesTests
{
    [TestFixture]
    public class RequestEmploymentCheckForEmployerPayeSchemesTests
    {
        private RequestEmploymentCheckForEmployerPayeSchemesCommand _target;
        private Mock<IAccountApiClient> _accountApiClient;
        private Mock<IMessagePublisher> _messagePublisher;
        private Mock<IProviderCommitmentsApi> _commitmentsApi;
        private Mock<ISubmissionEventRepository> _repository;
        private Mock<IEventsApi> _eventsApi;

        [SetUp]
        public void SetUp()
        {
            _accountApiClient = new Mock<IAccountApiClient>();
            _messagePublisher = new Mock<IMessagePublisher>();
            _commitmentsApi = new Mock<IProviderCommitmentsApi>();
            _repository = new Mock<ISubmissionEventRepository>();
            _eventsApi = new Mock<IEventsApi>();
            _target = new RequestEmploymentCheckForEmployerPayeSchemesCommand(_messagePublisher.Object, _accountApiClient.Object, _commitmentsApi.Object, _repository.Object, _eventsApi.Object, Mock.Of<ILog>());
        }

        [Test]
        public async Task WhenAnEmploymentCheckIsRequestedThenACheckIsRequestedForThePayeSchemesLinkedToTheEmployersAccount()
        {
            var request = new RequestEmploymentCheckForEmployerPayeSchemesRequest("AB12345C", 1324, 6543, 4353443, DateTime.Now.AddYears(-1));
            var expectedAccountId = 349875;
            _commitmentsApi.Setup(x => x.GetProviderApprenticeship(request.Ukprn, request.ApprenticeshipId)).ReturnsAsync(new Apprenticeship { EmployerAccountId = expectedAccountId });
            var accountPayeSchemes = new List<ResourceViewModel>
            {
                new ResourceViewModel {Id = "ABC/123"},
                new ResourceViewModel {Id = "ZZZ/999"}
            };
            var account = new AccountDetailViewModel { PayeSchemes = new ResourceList(accountPayeSchemes) };

            _accountApiClient.Setup(x => x.GetAccount(expectedAccountId)).ReturnsAsync(account);

            await _target.Handle(request);

            _messagePublisher.Verify(
                x => x.PublishAsync(It.Is<EmploymentCheckRequiredForAccountMessage>(y =>
                    y.Uln == request.Uln && y.ActualStartDate == request.ActualStartDate &&
                    y.EmployerAccountId == expectedAccountId &&
                    y.NationalInsuranceNumber == request.NationalInsuranceNumber && y.Ukprn == request.Ukprn &&
                    y.PayeSchemes.SequenceEqual(accountPayeSchemes.Select(z => z.Id)))), Times.Once());
        }

        [Test]
        public async Task WhenTheCommitmentHasADifferentProviderIdToThRequestThenANegativeEmploymentCheckResultIsStored()
        {
            var request = new RequestEmploymentCheckForEmployerPayeSchemesRequest("AB12345C", 1324, 6543, 4353443, DateTime.Now.AddYears(-1));
            _commitmentsApi.Setup(x => x.GetProviderApprenticeship(request.Ukprn, request.ApprenticeshipId)).Throws(new HttpRequestException("SAD STRING THAT CONTAINS 401 SO WE CAN DETERMINE THE STATUS CODE! :("));

            await _target.Handle(request);

            _repository.Verify(x => x.StoreEmploymentCheckResult(It.Is<PreviousHandledSubmissionEvent>(y => y.Uln == request.Uln && y.NiNumber == request.NationalInsuranceNumber && !y.PassedValidationCheck)), Times.Once);
            _eventsApi.Verify(x => x.CreateGenericEvent(It.Is<GenericEvent>(y => !string.IsNullOrWhiteSpace(y.Payload) && y.Type == "EmploymentCheckCompleteEvent")), Times.Once);
        }
    }
}
