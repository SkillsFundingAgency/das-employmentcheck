using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.NationalInsuranceNumberTests.WithLogging
{
    public class WhenGet
    {
        private NationalInsuranceNumberServiceWithLogging _sut;
        private Fixture _fixture;
        private Mock<INationalInsuranceNumberService> _nationalInsuranceNumberServiceMock;
        private Mock<ILogger<NationalInsuranceNumberService>> _mockLogger;
        private Mock<IDataCollectionsResponseRepository> _repositoryMock;

        private Data.Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _nationalInsuranceNumberServiceMock = new Mock<INationalInsuranceNumberService>();
            _mockLogger = new Mock<ILogger<NationalInsuranceNumberService>>();
            _repositoryMock = new Mock<IDataCollectionsResponseRepository>();

            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();

            _sut = new NationalInsuranceNumberServiceWithLogging(
                _nationalInsuranceNumberServiceMock.Object,
                _mockLogger.Object,
                _repositoryMock.Object
                );
        }

        [Test]
        public async Task Then_the_call_is_passed_to_the_service()
        {
            //Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");

            //Act
            await _sut.Get(request);

            // Assert
            _nationalInsuranceNumberServiceMock.Verify(m => m.Get(request), Times.Once);
        }

        public async Task Then_Error_Response_is_saved_on_exception()
        {
            // Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");
            var exception = _fixture.Create<Exception>();
            _nationalInsuranceNumberServiceMock.Setup(_ => _.Get(request))
                .ThrowsAsync(exception);

            // Act
            await _sut.Get(request);

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

        public async Task Then_null_is_returned_on_exception()
        {
            // Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");
            var exception = _fixture.Create<Exception>();
            _nationalInsuranceNumberServiceMock.Setup(_ => _.Get(request))
                .ThrowsAsync(exception);

            // Act
            var result = await _sut.Get(request);

            // Assert
            result.Should().BeNull();
        }

        public async Task Then_Error_is_logged_on_exception()
        {
            // Arrange
            var request = new NationalInsuranceNumberRequest(_employmentCheck, "2021");
            var exception = _fixture.Create<Exception>();
            _nationalInsuranceNumberServiceMock.Setup(_ => _.Get(request))
                .ThrowsAsync(exception);

            // Act
            await _sut.Get(request);

            // Assert
            _mockLogger.Verify(l => l.LogError($"{nameof(NationalInsuranceNumberService)}: Exception occurred [{exception}]"), Times.Once());
        }
    }
}