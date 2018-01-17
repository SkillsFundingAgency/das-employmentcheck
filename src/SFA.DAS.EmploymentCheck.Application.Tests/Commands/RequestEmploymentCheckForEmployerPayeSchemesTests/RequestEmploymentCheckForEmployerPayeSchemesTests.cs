using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NLog;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Application.Commands.RequestEmploymentCheckForEmployerPayeSchemes;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.EmploymentCheck.Application.Tests.Commands.RequestEmploymentCheckForEmployerPayeSchemesTests
{
    [TestFixture]
    public class RequestEmploymentCheckForEmployerPayeSchemesTests
    {
        private RequestEmploymentCheckForEmployerPayeSchemesCommand _target;
        private Mock<IAccountApiClient> _accountApiClient;
        private Mock<IMessagePublisher> _messagePublisher;
        
        [SetUp]
        public void SetUp()
        {
            _accountApiClient = new Mock<IAccountApiClient>();
            _messagePublisher = new Mock<IMessagePublisher>();
            _target = new RequestEmploymentCheckForEmployerPayeSchemesCommand(_messagePublisher.Object, _accountApiClient.Object, Mock.Of<ILogger>());
        }

        [Test]
        public async Task WhenAnEmploymentCheckIsRequestedThenACheckIsRequestedForThePayeSchemesLinkedToTheEmployersAccount()
        {
            var request = new RequestEmploymentCheckForEmployerPayeSchemesRequest("AB12345C", 1324, 6543, 4353443, DateTime.Now.AddYears(-1));
            var accountPayeSchemes = new List<ResourceViewModel>
            {
                new ResourceViewModel {Id = "ABC/123"},
                new ResourceViewModel {Id = "ZZZ/999"}
            };
            var account = new AccountDetailViewModel { PayeSchemes = new ResourceList(accountPayeSchemes) };

            _accountApiClient.Setup(x => x.GetAccount(request.EmployerAccountId)).ReturnsAsync(account);

            await _target.Handle(request);

            _messagePublisher.Verify(
                x => x.PublishAsync(It.Is<EmploymentCheckRequiredForAccountMessage>(y =>
                    y.Uln == request.Uln && y.ActualStartDate == request.ActualStartDate &&
                    y.EmployerAccountId == request.EmployerAccountId &&
                    y.NationalInsuranceNumber == request.NationalInsuranceNumber && y.Ukprn == request.Ukprn &&
                    y.PayeSchemes.SequenceEqual(accountPayeSchemes.Select(z => z.Id)))), Times.Once());
        }
    }
}
