using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Commands.PerformEmploymentCheck;
using SFA.DAS.EmploymentCheck.Application.Gateways;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmploymentCheck.Application.Tests.Commands.PerformEmploymentCheckTests
{
    [TestFixture]
    public class PerformEmploymentCheckTests
    {
        private PerformEmploymentCheckCommand _target;
        private Mock<ISubmissionEventRepository> _repository;
        private Mock<IEventsApi> _eventsApi;
        private Mock<IHmrcGateway> _hmrcGateway;
        private PerformEmploymentCheckRequest _request;

        [SetUp]
        public void SetUp()
        {
            _hmrcGateway = new Mock<IHmrcGateway>();
            _repository = new Mock<ISubmissionEventRepository>();
            _eventsApi = new Mock<IEventsApi>();
            _target = new PerformEmploymentCheckCommand(_hmrcGateway.Object, _eventsApi.Object, _repository.Object, Mock.Of<ILog>());

            var payeSchemes = new List<string> { "ABC/123", "ZZZ/999", "DFH/124" };
            _request = new PerformEmploymentCheckRequest("AA123456C", 123456, 543, 765, DateTime.Now.AddDays(-1), payeSchemes);
        }

        [Test]
        public async Task WhenTheEmploymentCheckPassesThenTheResultIsStoredAndAnEventIsCreated()
        {
            _hmrcGateway.Setup(x => x.IsNationalInsuranceNumberRelatedToPayeScheme(_request.PayeSchemes.ElementAt(0), _request.NationalInsuranceNumber, _request.ActualStartDate)).ReturnsAsync(false);
            _hmrcGateway.Setup(x => x.IsNationalInsuranceNumberRelatedToPayeScheme(_request.PayeSchemes.ElementAt(1), _request.NationalInsuranceNumber, _request.ActualStartDate)).ReturnsAsync(true);

            await _target.Handle(_request);

            _hmrcGateway.Verify(x => x.IsNationalInsuranceNumberRelatedToPayeScheme(It.IsAny<string>(), _request.NationalInsuranceNumber, _request.ActualStartDate), Times.Exactly(2));
            _repository.Verify(
                x => x.StoreEmploymentCheckResult(It.Is<PreviousHandledSubmissionEvent>(y => y.Uln == _request.Uln && y.NiNumber == _request.NationalInsuranceNumber && y.PassedValidationCheck)),
                Times.Once);
            _eventsApi.Verify(
                x => x.CreateGenericEvent<EmploymentCheckCompleteEvent>(It.Is<GenericEvent<EmploymentCheckCompleteEvent>>(y =>
                    y.Payload.Uln == _request.Uln && y.Payload.CheckDate.Date == DateTime.Now.Date && y.Payload.EmployerAccountId == _request.EmployerAccountId &&
                    y.Payload.NationalInsuranceNumber == _request.NationalInsuranceNumber && y.Payload.Ukprn == _request.Ukprn && y.Payload.CheckPassed)), Times.Once);
        }

        [Test]
        public async Task WhenTheEmploymentCheckFailsThenTheResultIsStoredAndAnEventIsCreated()
        {
            _hmrcGateway.Setup(x => x.IsNationalInsuranceNumberRelatedToPayeScheme(_request.PayeSchemes.ElementAt(0), _request.NationalInsuranceNumber, _request.ActualStartDate)).ReturnsAsync(false);
            _hmrcGateway.Setup(x => x.IsNationalInsuranceNumberRelatedToPayeScheme(_request.PayeSchemes.ElementAt(1), _request.NationalInsuranceNumber, _request.ActualStartDate)).ReturnsAsync(false);
            _hmrcGateway.Setup(x => x.IsNationalInsuranceNumberRelatedToPayeScheme(_request.PayeSchemes.ElementAt(2), _request.NationalInsuranceNumber, _request.ActualStartDate)).ReturnsAsync(false);

            await _target.Handle(_request);

            _hmrcGateway.Verify(x => x.IsNationalInsuranceNumberRelatedToPayeScheme(It.IsAny<string>(), _request.NationalInsuranceNumber, _request.ActualStartDate), Times.Exactly(3));
            _repository.Verify(
                x => x.StoreEmploymentCheckResult(It.Is<PreviousHandledSubmissionEvent>(y => y.Uln == _request.Uln && y.NiNumber == _request.NationalInsuranceNumber && !y.PassedValidationCheck)),
                Times.Once);
            _eventsApi.Verify(
                x => x.CreateGenericEvent<EmploymentCheckCompleteEvent>(It.Is<GenericEvent<EmploymentCheckCompleteEvent>>(y =>
                    y.Payload.Uln == _request.Uln && y.Payload.CheckDate.Date == DateTime.Now.Date && y.Payload.EmployerAccountId == _request.EmployerAccountId &&
                    y.Payload.NationalInsuranceNumber == _request.NationalInsuranceNumber && y.Payload.Ukprn == _request.Ukprn && !y.Payload.CheckPassed)), Times.Once);
        }
    }
}
