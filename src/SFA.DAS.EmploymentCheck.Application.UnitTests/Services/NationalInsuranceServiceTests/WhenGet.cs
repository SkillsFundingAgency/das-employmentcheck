using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Polly.Wrap;
using SFA.DAS.EmploymentCheck.Application.Services;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.NationalInsuranceNumberTests
{
    public class WhenGet
    {
        private NationalInsuranceNumberService _sut;
        private Fixture _fixture;
        private DataCollectionsApiConfiguration _apiApiConfiguration;
        private Mock<IDataCollectionsResponseRepository> _repositoryMock;
        private Mock<IApiOptionsRepository> _apiOptionsRepositoryMock;
        private Mock<IDataCollectionsApiClient<DataCollectionsApiConfiguration>> _apiClientMock;
        private ApiRetryOptions _settings;
        private Data.Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _apiApiConfiguration = new DataCollectionsApiConfiguration();
            _apiClientMock = new Mock<IDataCollectionsApiClient<DataCollectionsApiConfiguration>>();
            _repositoryMock = new Mock<IDataCollectionsResponseRepository>();
            _apiOptionsRepositoryMock = new Mock<IApiOptionsRepository>();

            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            _settings = new ApiRetryOptions
            {
                TooManyRequestsRetryCount = 10,
                TransientErrorRetryCount = 2,
                TransientErrorDelayInMs = 1
            };

            _apiOptionsRepositoryMock.Setup(r => r.GetOptions()).Returns(_settings);

            var retryPolicies = new ApiRetryPolicies(
              Mock.Of<ILogger<ApiRetryPolicies>>(),
              _apiOptionsRepositoryMock.Object);

            _sut = new NationalInsuranceNumberService(
                retryPolicies,
                _apiClientMock.Object,
                _apiApiConfiguration,
                _repositoryMock.Object
           );
        }

        [Test]
        public async Task Then_the_DC_api_is_called()
        {
            //Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");

            //Act
            await _sut.Get(request);

            // Assert
            _apiClientMock.Verify(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.Is<GetNationalInsuranceNumberRequest>(r =>
                 r.GetUrl == $"{_apiApiConfiguration.Path}/2021?ulns={_employmentCheck.Uln}")), Times.Once);
        }

        [Test]
        public async Task Then_Response_Is_Saved()
        {
            // Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");

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

            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.Is<GetNationalInsuranceNumberRequest>(
                    r => r.GetUrl == $"{_apiApiConfiguration.Path}/2021?ulns={_employmentCheck.Uln}")))
                .ReturnsAsync(httpResponse);

            // Act
            await _sut.Get(request);

            // Assert
            _repositoryMock.Verify(repository => repository.InsertOrUpdate(It.Is<DataCollectionsResponse>(
                        response => response.NiNumber == nino.NiNumber
                                    && response.Uln == _employmentCheck.Uln
                                    && response.ApprenticeEmploymentCheckId == _employmentCheck.Id
                                    && response.CorrelationId == _employmentCheck.CorrelationId
                                    && response.HttpResponse.StartsWith(httpResponse.ToString())
                                    && response.HttpStatusCode == (short)httpResponse.StatusCode
                    )
                )
                , Times.Once());
        }

        [Test]
        public async Task Then_ReturnedNiNo_Is_Returned_To_Caller()
        {
            // Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");
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

            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.Is<GetNationalInsuranceNumberRequest>(
                    r => r.GetUrl == $"{_apiApiConfiguration.Path}/2021?ulns={_employmentCheck.Uln}")))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.Get(request);

            // Assert
            result.Should().BeEquivalentTo(nino);
        }

        [Test]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Null_Response()
        {
            // Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");
            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync((HttpResponseMessage)null);

            // Act
            await _sut.Get(request);

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
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");
            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync((HttpResponseMessage)null);

            // Act
            var result = await _sut.Get(request);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Unsuccessful_Response(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(_fixture.Create<string>()),
                StatusCode = httpStatusCode
            };

            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            await _sut.Get(request);

            // Assert
            _repositoryMock.Verify(repository => repository.InsertOrUpdate(It.Is<DataCollectionsResponse>(
                        response => response.NiNumber == null
                                    && response.Uln == _employmentCheck.Uln
                                    && response.ApprenticeEmploymentCheckId == _employmentCheck.Id
                                    && response.CorrelationId == _employmentCheck.CorrelationId
                                    && response.HttpResponse.StartsWith(httpResponse.ToString())
                                    && response.HttpStatusCode == (short)httpResponse.StatusCode
                    )
                )
                , Times.Once());
        }


        [Test]
        public async Task Then_LearnerNiNumber_Is_Returned_To_Caller_In_Case_Of_Unsuccessful_Response()
        {
            // Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.BadRequest
            };

            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.Get(request);

            // Assert
            result.Should().NotBeNull();
            result.HttpStatusCode.Should().Be(httpResponse.StatusCode);
            result.NiNumber.Should().BeNull();
        }

        [Test]
        public async Task Then_LearnerNiNumber_With_Null_NiNumber_Is_Returned_In_Case_Of_Empty_Response()
        {
            // Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.OK
            };
            var learnerNiNumber = _fixture.Build<LearnerNiNumber>()
                .With(x => x.NiNumber, () => null)
                .With(x => x.HttpStatusCode, httpResponse.StatusCode)
                .Create();

            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.Get(request);

            // Assert
            result.Should().NotBeNull();
            result.HttpStatusCode.Should().Be(learnerNiNumber.HttpStatusCode);
            result.NiNumber.Should().BeNull();
        }
    }
}