﻿using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.LearnerServiceTests
{
    public class WhenGettingDbLearnerNiNumbers
    {
        private ILearnerService _sut;
        private Fixture _fixture;
        private Mock<IDataCollectionsResponseRepository> _repositoryMock;
        private Mock<INationalInsuranceNumberService> _nationalInsuranceNumberServiceMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _nationalInsuranceNumberServiceMock = new Mock<INationalInsuranceNumberService>();
            _repositoryMock = new Mock<IDataCollectionsResponseRepository>(MockBehavior.Strict);

            _sut = new LearnerService(
                _repositoryMock.Object,
                _nationalInsuranceNumberServiceMock.Object);
        }

        [Test]
        public async Task Then_GetDbNiNumbers_Is_Called()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            var dataCollectionsResponse = _fixture.Build<DataCollectionsResponse>().With(_ => _.Uln, employmentCheck.Uln).With(_ => _.HttpStatusCode, (short)HttpStatusCode.OK).Create();

            _repositoryMock
                .Setup(r => r.GetByEmploymentCheckId(It.Is<long>(x => x == employmentCheck.Id)))
                .ReturnsAsync(dataCollectionsResponse);

            // Act
            var actual = await _sut.GetDbNiNumber(employmentCheck);

            // Assert
            actual.Uln.Should().Be(employmentCheck.Uln);
            actual.NiNumber.Should().Be(dataCollectionsResponse.NiNumber);
            actual.HttpStatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}