﻿using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheckServiceTests
{
    public class WhenGettingEmploymentCheck
    {
        private IEmploymentCheckService _sut;
        private Fixture _fixture;
        private Mock<IEmploymentCheckRepository> _repositoryMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _repositoryMock = new Mock<IEmploymentCheckRepository>();

            _sut = new EmploymentCheckService(
                _repositoryMock.Object,
                Mock.Of<IEmploymentCheckCacheRequestRepository>(),
                Mock.Of<IUnitOfWork>(), Mock.Of<ILogger<EmploymentCheckService>>());
        }

        [Test]
        public async Task Then_The_Repository_Is_Called()
        {
            // Act
            await _sut.GetEmploymentCheck();

            // Assert
            _repositoryMock.Verify(x => x.GetEmploymentCheck(), Times.AtLeastOnce());
        }

        [Test]
        public async Task And_The_Repository_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();

            _repositoryMock.Setup(x => x.GetEmploymentCheck())
                .ReturnsAsync(employmentCheck);

            // Act
            var result = await _sut.GetEmploymentCheck();

            // Assert
            Assert.AreEqual(employmentCheck, result);
        }
    }
}