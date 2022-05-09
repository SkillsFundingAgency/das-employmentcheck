using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.LearnerServiceTests
{
    public class WhenGetNiNumber
    {
        private ILearnerService _sut;
        private Fixture _fixture;
        private Mock<IDataCollectionsResponseRepository> _repositoryMock;
        private Mock<IDataCollectionsApiClient<DataCollectionsApiConfiguration>> _apiClientMock;
        private Data.Models.EmploymentCheck _employmentCheck;
        private Mock<IApiOptionsRepository> _apiOptionsRepositoryMock;
        private ApiRetryOptions _settings;
        private Mock<ILogger<LearnerService>> _logger;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _logger = new Mock<ILogger<LearnerService>>();
            _apiClientMock = new Mock<IDataCollectionsApiClient<DataCollectionsApiConfiguration>>();
            _repositoryMock = new Mock<IDataCollectionsResponseRepository>();
            _employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Create();

            _apiOptionsRepositoryMock = new Mock<IApiOptionsRepository>();

            _settings = new ApiRetryOptions
            {
                TooManyRequestsRetryCount = 10,
                TransientErrorRetryCount = 2,
                TransientErrorDelayInMs = 1
            };

            _apiOptionsRepositoryMock.Setup(r => r.GetOptions()).ReturnsAsync(_settings);

            var retryPolicies = new ApiRetryPolicies(
                Mock.Of<ILogger<ApiRetryPolicies>>(),
                _apiOptionsRepositoryMock.Object);

            _sut = new LearnerService(
                _apiClientMock.Object,
                _repositoryMock.Object,
                retryPolicies,
                _logger.Object
            );
        }

        [Test]
        public async Task Then_DcApi_Is_Called()
        {
            // Act
            await _sut.GetNiNumber(_employmentCheck);

            // Assert
            _apiClientMock.Verify(_ => _.Get(It.Is<GetNationalInsuranceNumberRequest>(r =>
                r.GetUrl == $"/api/v1/ilr-data/learnersNi/2122?ulns={_employmentCheck.Uln}")));
        }

        [Test]
        public async Task Then_Response_Is_Saved()
        {
            // Arrange
            var nino = _fixture.Build<LearnerNiNumber>()
                .With(n => n.Uln, _employmentCheck.Uln)
                .Without(n => n.NiNumber)
                .Create();
            nino.NiNumber = _fixture.Create<string>()[..20];

            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent($"[{{\"uln\":{nino.Uln},\"niNumber\":\"{nino.NiNumber}\"}},{{\"uln\":{nino.Uln},\"niNumber\":\"{{NOT_USED}}\"}}]"),
                StatusCode = HttpStatusCode.OK
            };

            _apiClientMock.Setup(_ => _.Get(It.Is<GetNationalInsuranceNumberRequest>(
                    r => r.GetUrl == $"/api/v1/ilr-data/learnersNi/2122?ulns={_employmentCheck.Uln}")))
                .ReturnsAsync(httpResponse);

            // Act
            await _sut.GetNiNumber(_employmentCheck);

            // Assert
            _repositoryMock.Verify(repository => repository.InsertOrUpdate(It.Is<DataCollectionsResponse>(
                        response => response.NiNumber == nino.NiNumber
                                    && response.Uln == _employmentCheck.Uln
                                    && response.ApprenticeEmploymentCheckId == _employmentCheck.Id
                                    && response.CorrelationId == _employmentCheck.CorrelationId
                                    && response.HttpResponse == httpResponse.ToString()
                                    && response.HttpStatusCode == (short)httpResponse.StatusCode
                    )
                )
                , Times.Once());
        }

        [Test]
        public async Task Then_ReturnedNiNo_Is_Returned_To_Caller()
        {
            // Arrange
            var nino = _fixture.Build<LearnerNiNumber>()
                .With(n => n.Uln, _employmentCheck.Uln)
                .Without(n => n.NiNumber)
                .With(n => n.HttpStatusCode, HttpStatusCode.OK)
                .Create();
            nino.NiNumber = _fixture.Create<string>()[..20];

            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent($"[{{\"uln\":{nino.Uln},\"niNumber\":\"{nino.NiNumber}\"}}]"),
                StatusCode = HttpStatusCode.OK
            };

            _apiClientMock.Setup(_ => _.Get(It.Is<GetNationalInsuranceNumberRequest>(
                    r => r.GetUrl == $"/api/v1/ilr-data/learnersNi/2122?ulns={_employmentCheck.Uln}")))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetNiNumber(_employmentCheck);

            // Assert
            result.Should().BeEquivalentTo(nino);
        }

        [Test]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Null_Response()
        {
            // Arrange
            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync((HttpResponseMessage)null);

            // Act
            await _sut.GetNiNumber(_employmentCheck);

            // Assert
            _repositoryMock.Verify(repository => repository.InsertOrUpdate(It.Is<DataCollectionsResponse>(
                        response => response.NiNumber == null
                                    && response.Uln == _employmentCheck.Uln
                                    && response.ApprenticeEmploymentCheckId == _employmentCheck.Id
                                    && response.CorrelationId == _employmentCheck.CorrelationId
                                    && response.HttpResponse == ""
                                    && response.HttpStatusCode == (short)HttpStatusCode.InternalServerError
                    )
                )
                , Times.Once());
        }

        [Test]
        public async Task Then_Null_Is_Returned_To_Caller_In_Case_Of_Null_Response()
        {
            // Arrange
            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync((HttpResponseMessage)null);

            // Act
            var result = await _sut.GetNiNumber(_employmentCheck);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Unsuccessful_Response()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(_fixture.Create<string>()),
                StatusCode = HttpStatusCode.BadRequest
            };

            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync(httpResponse);
            
                // Act
                await _sut.GetNiNumber(_employmentCheck);
            
            

            // Assert
            _repositoryMock.Verify(repository => repository.InsertOrUpdate(It.Is<DataCollectionsResponse>(
                        response => response.NiNumber == null
                                    && response.Uln == _employmentCheck.Uln
                                    && response.ApprenticeEmploymentCheckId == _employmentCheck.Id
                                    && response.CorrelationId == _employmentCheck.CorrelationId
                                    && response.HttpResponse == httpResponse.ToString()
                                    && response.HttpStatusCode == (short)httpResponse.StatusCode
                    )
                )
                , Times.Once());
        }

        [Test]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Unauthorized_Or_InternalSerever_Response(HttpStatusCode httpStatusCode)
        {
            //This response will retry using the Settings.TokenRetrievalRetryCount

            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(_fixture.Create<string>()),
                StatusCode = httpStatusCode
            };

            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            await _sut.GetNiNumber(_employmentCheck);



            // Assert
            _repositoryMock.Verify(repository => repository.InsertOrUpdate(It.Is<DataCollectionsResponse>(
                        response => response.NiNumber == null
                                    && response.Uln == _employmentCheck.Uln
                                    && response.ApprenticeEmploymentCheckId == _employmentCheck.Id
                                    && response.CorrelationId == _employmentCheck.CorrelationId
                                    && response.HttpResponse == httpResponse.ToString()
                                    && response.HttpStatusCode == (short)httpResponse.StatusCode
                    )
                )
                , Times.Once());

            _logger.Verify(m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"{nameof(LearnerService)}: Throwing exception for retry")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtMost(3));
            
        }

        [Test]
        public async Task Then_LearnerNiNumber_Is_Returned_To_Caller_In_Case_Of_Unsuccessful_Response()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.BadRequest
            };

            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetNiNumber(_employmentCheck);

            // Assert
            result.Should().NotBeNull();
            result.HttpStatusCode.Should().Be(httpResponse.StatusCode);
            result.NiNumber.Should().BeNull();
        }

        [Test]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Unexpected_Exception()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ThrowsAsync(exception);

            // Act
            await _sut.GetNiNumber(_employmentCheck);

            // Assert
            _repositoryMock.Verify(repository => repository.InsertOrUpdate(It.Is<DataCollectionsResponse>(
                        response => response.NiNumber == null
                                    && response.Uln == _employmentCheck.Uln
                                    && response.ApprenticeEmploymentCheckId == _employmentCheck.Id
                                    && response.CorrelationId == _employmentCheck.CorrelationId
                                    && response.HttpResponse == exception.Message
                                    && response.HttpStatusCode == (short)HttpStatusCode.InternalServerError
                    )
                )
                , Times.Once());
        }

        [Test]
        public async Task Then_Null_Is_Returned_To_Caller_In_Case_Of_Unexpected_Exception()
        {
            // Arrange
            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ThrowsAsync(_fixture.Create<Exception>());

            // Act
            var result = await _sut.GetNiNumber(_employmentCheck);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Then_LearnerNiNumber_With_Null_NiNumber_Is_Returned_In_Case_Of_Empty_Response()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.OK
            };
            var learnerNiNumber = _fixture.Build<LearnerNiNumber>()
                .With(x => x.NiNumber, () => null )
                .With(x => x.HttpStatusCode, httpResponse.StatusCode)
                .Create();

            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetNiNumber(_employmentCheck);

            // Assert
            result.Should().NotBeNull();
            result.HttpStatusCode.Should().Be(learnerNiNumber.HttpStatusCode);
            result.NiNumber.Should().BeNull();
        }
    }
}