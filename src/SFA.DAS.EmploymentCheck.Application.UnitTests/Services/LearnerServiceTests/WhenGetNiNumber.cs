using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
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

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _apiClientMock = new Mock<IDataCollectionsApiClient<DataCollectionsApiConfiguration>>();
            _repositoryMock = new Mock<IDataCollectionsResponseRepository>();
            _employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Create();

            _sut = new LearnerService(
                _apiClientMock.Object,
                _repositoryMock.Object
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
        public async Task Then_Default_NiNo_Is_Returned_To_Caller_In_Case_Of_Null_Response()
        {
            // Arrange
            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync((HttpResponseMessage)null);

            // Act
            var result = await _sut.GetNiNumber(_employmentCheck);

            // Assert
            result.Should().BeEquivalentTo(new LearnerNiNumber());
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
        public async Task Then_Default_NiNo_Is_Returned_To_Caller_In_Case_Of_Unsuccessful_Response()
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
            var result = await _sut.GetNiNumber(_employmentCheck);

            // Assert
            result.Should().BeEquivalentTo(new LearnerNiNumber());
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
        public async Task Then_Default_NiNo_Is_Returned_To_Caller_In_Case_Of_Unexpected_Exception()
        {
            // Arrange
            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ThrowsAsync(_fixture.Create<Exception>());

            // Act
            var result = await _sut.GetNiNumber(_employmentCheck);

            // Assert
            result.Should().BeEquivalentTo(new LearnerNiNumber());
        }

        [Test]
        public async Task Then_Empty_String_As_NiNo_Is_Returned_In_Case_Of_Empty_Response()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.OK
            };

            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetNationalInsuranceNumberRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetNiNumber(_employmentCheck);

            // Assert
            result.Should().BeEquivalentTo(new LearnerNiNumber { Uln = _employmentCheck.Uln, NiNumber = string.Empty});
        }
    }
}