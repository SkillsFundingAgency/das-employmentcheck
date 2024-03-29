﻿using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheckServiceTests
{
    public class WhenGettingEmploymentCheckCacheRequest
    {
        private IEmploymentCheckService _sut;
        private Fixture _fixture;
        private Mock<IEmploymentCheckRepository> _employmentCheckRepositoryMock;
        private Mock<IEmploymentCheckCacheRequestRepository> _employmentCheckCashRequestRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckRepositoryMock = new Mock<IEmploymentCheckRepository>();
            _employmentCheckCashRequestRepositoryMock = new Mock<IEmploymentCheckCacheRequestRepository>();

            _sut = new EmploymentCheckService(
                _employmentCheckRepositoryMock.Object,
                _employmentCheckCashRequestRepositoryMock.Object,
                Mock.Of<IUnitOfWork>(), Mock.Of<ILogger<EmploymentCheckService>>());
        }

        [Test]
        public async Task Then_The_Repository_Is_Called()
        {
            // Act
            await _sut.GetEmploymentCheckCacheRequests();

            // Assert
            _employmentCheckCashRequestRepositoryMock.Verify(x => x.GetEmploymentCheckCacheRequests(), Times.AtLeastOnce);
        }

        [Test]
        public async Task And_The_Repository_Returns_Null_EmploymentCheckCacheRequest_Then_Null_Is_Returned()
        {
            // Arrange
            _employmentCheckCashRequestRepositoryMock.Setup(x => x.GetEmploymentCheckCacheRequests())
                .ReturnsAsync((EmploymentCheckCacheRequest[])null);

            // Act
            var result = await _sut.GetEmploymentCheckCacheRequests();

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task And_The_Repository_Returns_EmploymentCheckCacheRequest_Then_It_Is_Returned()
        {
            // Arrange
            var employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest[]>();

            _employmentCheckCashRequestRepositoryMock.Setup(x => x.GetEmploymentCheckCacheRequests())
                .ReturnsAsync(employmentCheckCacheRequest);

            // Act
            var result = await _sut.GetEmploymentCheckCacheRequests();

            // Assert
            Assert.AreEqual(employmentCheckCacheRequest, result);
        }
    }
}