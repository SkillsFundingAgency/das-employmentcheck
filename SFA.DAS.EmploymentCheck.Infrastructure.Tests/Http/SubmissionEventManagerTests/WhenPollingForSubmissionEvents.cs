using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Models;
using SFA.DAS.EmploymentCheck.Domain;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;
using SFA.DAS.EmploymentCheck.Infrastructure.Http;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Tests.Http.SubmissionEventManagerTests
{
    [TestFixture]
    public class WhenPollingForSubmissionEvents
    {
        private SubmissionEventManager _submissionEventManager;
        private Mock<IHttpClientWrapper> _httpClientWrapper;
        private Mock<ISubmissionEventOrchestrator> _orchestrator;
        private Mock<IEmploymentCheckConfiguration> _configuration;
        private const string ApiUrl = "http://www.site.com?sinceEventId={0}";
        private const string GetRequestPayload = "{\"PageNumber\":1,\"TotalNumberOfPages\":1,\"Items\":[{\"Id\":1,\"IlrFileName\":\"ILR-26186358-1718-20171020-114025-01.xml\",\"FileDateTime\":\"2017-10-20T00:00:00\",\"SubmittedDateTime\":\"2017-10-20T11:44:25\",\"ComponentVersionNumber\":1,\"Ukprn\":26186358,\"Uln\":1000000001,\"StandardCode\":34,\"ProgrammeType\":0,\"FrameworkCode\":0,\"PathwayCode\":0,\"ActualStartDate\":\"2018-01-20T00:00:00\",\"PlannedEndDate\":\"2019-02-20T00:00:00\",\"TrainingPrice\":12000.00000,\"EndpointAssessorPrice\":3000.00000,\"NiNumber\":\"AB123456B\",\"ApprenticeshipId\":567577,\"AcademicYear\":\"1617\",\"EmployerReferenceNumber\":123456},{\"Id\":2,\"IlrFileName\":\"ILR-77030639-1718-20171020-114028-01.xml\",\"FileDateTime\":\"2017-10-20T00:00:00\",\"SubmittedDateTime\":\"2017-10-20T11:44:28\",\"ComponentVersionNumber\":1,\"Ukprn\":77030639,\"Uln\":1000000000,\"StandardCode\":34,\"ProgrammeType\":0,\"FrameworkCode\":0,\"PathwayCode\":0,\"ActualStartDate\":\"2018-01-20T00:00:00\",\"PlannedEndDate\":\"2019-02-20T00:00:00\",\"TrainingPrice\":12000.00000,\"EndpointAssessorPrice\":3000.00000,\"NiNumber\":\"AB123456A\",\"ApprenticeshipId\":567577,\"AcademicYear\":\"1617\",\"EmployerReferenceNumber\":123456}]}";

        [SetUp]
        public void Arrange()
        {
            _httpClientWrapper = new Mock<IHttpClientWrapper>();
            _configuration = new Mock<IEmploymentCheckConfiguration>();
            _orchestrator = new Mock<ISubmissionEventOrchestrator>();
            _submissionEventManager = new SubmissionEventManager(_httpClientWrapper.Object, _orchestrator.Object, _configuration.Object);
            _configuration.Setup(o => o.SubmissionEventApiAddress).Returns(ApiUrl);
            var msg = new HttpResponseMessage {Content = new StringContent(GetRequestPayload, Encoding.UTF8, Constants.ContentTypeValue)};
            _httpClientWrapper.Setup(o => o.GetAsync(It.IsAny<Uri>(), It.IsAny<string>())).ReturnsAsync(msg);
            _httpClientWrapper.Setup(o => o.ReadResponse<PageOfResults<NinoChangedEventMessage>>(It.IsAny<HttpResponseMessage>())).ReturnsAsync(JsonConvert.DeserializeObject<PageOfResults<NinoChangedEventMessage>>(GetRequestPayload));
            _orchestrator.Setup(o => o.GetPreviouslyHandledSubmissionEvents(It.IsAny<string>()))
                .ReturnsAsync(new OrchestratorResponse<PreviouslyHandledSubmissionEventViewModel>
                {
                    Data = new PreviouslyHandledSubmissionEventViewModel
                    {
                    Events    = new[]
                        {
                            new PreviousHandledSubmissionEvent
                            {
                                Uln = "1000000001",
                                NiNumber = "AB123456B",
                                PassedValidationCheck = false
                            },
                            new PreviousHandledSubmissionEvent
                            {
                                Uln = "1000000000",
                                NiNumber = "AB123456A",
                                PassedValidationCheck = true
                            }
                        }
                    }
                });
        }

        [Test]
        public async Task ThenTheApiIsCalled()
        {
            await InvokePollSubmissionEventsMethod();

            _httpClientWrapper.Verify(o => o.GetAsync(It.IsAny<Uri>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ThenTheApiPassesTheEventIdIntoTheApiCall()
        {
            await InvokePollSubmissionEventsMethod();
            var url = string.Format(ApiUrl, 0);

            _httpClientWrapper.Verify(o => o.GetAsync(url.ToUri(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ThenTheHttpResponseIsParsed()
        {
            await InvokePollSubmissionEventsMethod();

            _httpClientWrapper.Verify(o => o.ReadResponse<PageOfResults<NinoChangedEventMessage>>(It.IsAny<HttpResponseMessage>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task ThenTheLastProcessedEventIdIsUpdated()
        {
            await InvokePollSubmissionEventsMethod();

            Assert.AreEqual(1, _submissionEventManager.LastProcessedEventId);
        }

        [Test]
        public async Task ThenThePreviousValidatedUlnsAreRetrieved()
        {
            await InvokePollSubmissionEventsMethod();

            _orchestrator.Verify(o => o.GetPreviouslyHandledSubmissionEvents(It.Is<string>(s => s == "[\"1000000001\",\"1000000000\"]")), Times.Once);
        }
        
        private async Task InvokePollSubmissionEventsMethod()
        {
            await _submissionEventManager.PollSubmissionEvents();
        }
    }
}
