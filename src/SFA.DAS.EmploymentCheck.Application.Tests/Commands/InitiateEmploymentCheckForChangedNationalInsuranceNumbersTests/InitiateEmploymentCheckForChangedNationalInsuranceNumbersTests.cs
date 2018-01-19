using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Commands.InitiateEmploymentCheckForChangedNationalInsuranceNumbers;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.EmploymentCheck.Application.Tests.Commands.InitiateEmploymentCheckForChangedNationalInsuranceNumbersTests
{
    [TestFixture]
    public class InitiateEmploymentCheckForChangedNationalInsuranceNumbersTests
    {
        private InitiateEmploymentCheckForChangedNationalInsuranceNumbersCommand _target;
        private Mock<ISubmissionEventRepository> _repository;
        private Mock<IPaymentsEventsApiClient> _eventsApi;
        private Mock<IMessagePublisher> _messagePublisher;
        private SubmissionEvent _submissionEvent;

        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<ISubmissionEventRepository>();
            _eventsApi = new Mock<IPaymentsEventsApiClient>();
            _messagePublisher = new Mock<IMessagePublisher>();
            _target = new InitiateEmploymentCheckForChangedNationalInsuranceNumbersCommand(_repository.Object, _eventsApi.Object, _messagePublisher.Object, Mock.Of<ILog>());

            _submissionEvent = new SubmissionEvent { Uln = 1234565, NiNumber = "JA123456C", Id = 124, ActualStartDate = DateTime.Now.AddYears(-1), EmployerReferenceNumber = 123456, Ukprn = 5465 };
        }

        [Test]
        public async Task WhenThereAreNoNewEventsToProcessThenNoEmploymentChecksAreRequested()
        {
            var previousEventId = 123;
            _repository.Setup(x => x.GetLastProcessedEventId()).ReturnsAsync(previousEventId);

            _eventsApi.Setup(x => x.GetSubmissionEvents(previousEventId, null, 0L, 1)).ReturnsAsync(new PageOfResults<SubmissionEvent> { Items = new SubmissionEvent[] {} });

            await _target.Handle(new InitiateEmploymentCheckForChangedNationalInsuranceNumbersRequest());

            _messagePublisher.Verify(x => x.PublishAsync(It.IsAny<EmploymentCheckRequiredForApprenticeMessage>()), Times.Never());
        }

        [Test]
        public async Task WhenTheNationalInsuranceHasntChangedAndTheCheckHasPreviousPassedThenACheckIsNotRequested()
        {
            var previousEventId = 123;
            _repository.Setup(x => x.GetLastProcessedEventId()).ReturnsAsync(previousEventId);

            _eventsApi.Setup(x => x.GetSubmissionEvents(previousEventId, null, 0L, 1)).ReturnsAsync(new PageOfResults<SubmissionEvent> { Items = new[] { _submissionEvent } });

            var previouslyHandledSubmissionEvent = new PreviousHandledSubmissionEvent { Uln = _submissionEvent.Uln, NiNumber = _submissionEvent.NiNumber, PassedValidationCheck = true };
            _repository.Setup(x => x.GetPreviouslyHandledSubmissionEvents(It.Is<IEnumerable<long>>(y => y.First() == _submissionEvent.Uln))).ReturnsAsync(new List<PreviousHandledSubmissionEvent> { previouslyHandledSubmissionEvent });

            await _target.Handle(new InitiateEmploymentCheckForChangedNationalInsuranceNumbersRequest());

            _messagePublisher.Verify(x => x.PublishAsync(It.IsAny<EmploymentCheckRequiredForApprenticeMessage>()),Times.Never());
        }

        [Test]
        public async Task WhenTheNationalInsuranceHasChangedThenACheckIsRequested()
        {
            var previousEventId = 123;
            _repository.Setup(x => x.GetLastProcessedEventId()).ReturnsAsync(previousEventId);

            _eventsApi.Setup(x => x.GetSubmissionEvents(previousEventId, null, 0L, 1)).ReturnsAsync(new PageOfResults<SubmissionEvent> { Items = new[] { _submissionEvent } });

            var previouslyHandledSubmissionEvent = new PreviousHandledSubmissionEvent { Uln = _submissionEvent.Uln, NiNumber = "AA123456A", PassedValidationCheck = true };
            _repository.Setup(x => x.GetPreviouslyHandledSubmissionEvents(It.Is<IEnumerable<long>>(y => y.First() == _submissionEvent.Uln))).ReturnsAsync(new List<PreviousHandledSubmissionEvent> { previouslyHandledSubmissionEvent });

            await _target.Handle(new InitiateEmploymentCheckForChangedNationalInsuranceNumbersRequest());

            _messagePublisher.Verify(
                x => x.PublishAsync(It.Is<EmploymentCheckRequiredForApprenticeMessage>(y =>
                    y.Uln == _submissionEvent.Uln &&
                    y.ActualStartDate == _submissionEvent.ActualStartDate &&
                    y.EmployerAccountId == _submissionEvent.EmployerReferenceNumber &&
                    y.NationalInsuranceNumber == _submissionEvent.NiNumber &&
                    y.Ukprn == _submissionEvent.Ukprn)),
                Times.Once());
        }

        [Test]
        public async Task WhenTheNationalInsuranceHasNotChangedButThePreviousCheckFailedThenACheckIsRequested()
        {
            var previousEventId = 123;
            _repository.Setup(x => x.GetLastProcessedEventId()).ReturnsAsync(previousEventId);

            _eventsApi.Setup(x => x.GetSubmissionEvents(previousEventId, null, 0L, 1)).ReturnsAsync(new PageOfResults<SubmissionEvent> { Items = new[] { _submissionEvent } });

            var previouslyHandledSubmissionEvent = new PreviousHandledSubmissionEvent { Uln = _submissionEvent.Uln, NiNumber = _submissionEvent.NiNumber, PassedValidationCheck = false };
            _repository.Setup(x => x.GetPreviouslyHandledSubmissionEvents(It.Is<IEnumerable<long>>(y => y.First() == _submissionEvent.Uln))).ReturnsAsync(new List<PreviousHandledSubmissionEvent> { previouslyHandledSubmissionEvent });

            await _target.Handle(new InitiateEmploymentCheckForChangedNationalInsuranceNumbersRequest());

            _messagePublisher.Verify(
                x => x.PublishAsync(It.Is<EmploymentCheckRequiredForApprenticeMessage>(y =>
                    y.Uln == _submissionEvent.Uln &&
                    y.ActualStartDate == _submissionEvent.ActualStartDate &&
                    y.EmployerAccountId == _submissionEvent.EmployerReferenceNumber &&
                    y.NationalInsuranceNumber == _submissionEvent.NiNumber &&
                    y.Ukprn == _submissionEvent.Ukprn)),
                Times.Once());
        }

        [Test]
        public async Task WhenTheLearnerHasNotBeenCheckedBeforeThenACheckIsRequested()
        {
            var previousEventId = 123;
            _repository.Setup(x => x.GetLastProcessedEventId()).ReturnsAsync(previousEventId);

            _eventsApi.Setup(x => x.GetSubmissionEvents(previousEventId, null, 0L, 1)).ReturnsAsync(new PageOfResults<SubmissionEvent> { Items = new[] { _submissionEvent } });

            var previouslyHandledSubmissionEvent = new PreviousHandledSubmissionEvent { Uln = _submissionEvent.Uln + 1, NiNumber = _submissionEvent.NiNumber, PassedValidationCheck = true };
            _repository.Setup(x => x.GetPreviouslyHandledSubmissionEvents(It.Is<IEnumerable<long>>(y => y.First() == _submissionEvent.Uln))).ReturnsAsync(new List<PreviousHandledSubmissionEvent> { previouslyHandledSubmissionEvent });

            await _target.Handle(new InitiateEmploymentCheckForChangedNationalInsuranceNumbersRequest());

            _messagePublisher.Verify(
                x => x.PublishAsync(It.Is<EmploymentCheckRequiredForApprenticeMessage>(y =>
                    y.Uln == _submissionEvent.Uln &&
                    y.ActualStartDate == _submissionEvent.ActualStartDate &&
                    y.EmployerAccountId == _submissionEvent.EmployerReferenceNumber &&
                    y.NationalInsuranceNumber == _submissionEvent.NiNumber &&
                    y.Ukprn == _submissionEvent.Ukprn)),
                Times.Once());
        }

        [Test]
        public async Task WhenAnEventHasBeenProcessedThenItIsntProcessedAgain()
        {
            var previousEventId = 123;
            _repository.Setup(x => x.GetLastProcessedEventId()).ReturnsAsync(previousEventId);

            _eventsApi.Setup(x => x.GetSubmissionEvents(previousEventId, null, 0L, 1)).ReturnsAsync(new PageOfResults<SubmissionEvent> { Items = new[] { _submissionEvent }});

            await _target.Handle(new InitiateEmploymentCheckForChangedNationalInsuranceNumbersRequest());

            _repository.Verify(x => x.SetLastProcessedEvent(_submissionEvent.Id));
        }

        [Test]
        public async Task WhenNoEmployerIdIsProvidedThenACheckIsNotRequested()
        {
            var previousEventId = 123;
            _repository.Setup(x => x.GetLastProcessedEventId()).ReturnsAsync(previousEventId);

            _submissionEvent.EmployerReferenceNumber = null;
            _eventsApi.Setup(x => x.GetSubmissionEvents(previousEventId, null, 0L, 1)).ReturnsAsync(new PageOfResults<SubmissionEvent> { Items = new[] { _submissionEvent } });

            var previouslyHandledSubmissionEvent = new PreviousHandledSubmissionEvent { Uln = _submissionEvent.Uln + 1, NiNumber = _submissionEvent.NiNumber, PassedValidationCheck = true };
            _repository.Setup(x => x.GetPreviouslyHandledSubmissionEvents(It.Is<IEnumerable<long>>(y => y.First() == _submissionEvent.Uln))).ReturnsAsync(new List<PreviousHandledSubmissionEvent> { previouslyHandledSubmissionEvent });

            await _target.Handle(new InitiateEmploymentCheckForChangedNationalInsuranceNumbersRequest());

            _messagePublisher.Verify(x => x.PublishAsync(It.IsAny<EmploymentCheckRequiredForApprenticeMessage>()), Times.Never());
        }

        [Test]
        public async Task WhenNoActualStartDateIsProvidedThenACheckIsNotRequested()
        {
            var previousEventId = 123;
            _repository.Setup(x => x.GetLastProcessedEventId()).ReturnsAsync(previousEventId);

            _submissionEvent.ActualStartDate = null;
            _eventsApi.Setup(x => x.GetSubmissionEvents(previousEventId, null, 0L, 1)).ReturnsAsync(new PageOfResults<SubmissionEvent> { Items = new[] { _submissionEvent } });

            var previouslyHandledSubmissionEvent = new PreviousHandledSubmissionEvent { Uln = _submissionEvent.Uln + 1, NiNumber = _submissionEvent.NiNumber, PassedValidationCheck = true };
            _repository.Setup(x => x.GetPreviouslyHandledSubmissionEvents(It.Is<IEnumerable<long>>(y => y.First() == _submissionEvent.Uln))).ReturnsAsync(new List<PreviousHandledSubmissionEvent> { previouslyHandledSubmissionEvent });

            await _target.Handle(new InitiateEmploymentCheckForChangedNationalInsuranceNumbersRequest());

            _messagePublisher.Verify(x => x.PublishAsync(It.IsAny<EmploymentCheckRequiredForApprenticeMessage>()), Times.Never());
        }
    }
}
