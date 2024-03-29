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
    public class WhenSaveEmploymentCheck
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
        public async Task The_Repository_InsertOrUpdate_Is_Called()
        {
            // Arrange
            var expected = _fixture.Create<Data.Models.EmploymentCheck>();

            // Act
            await _sut.SaveEmploymentCheck(expected);

            // Assert
            _repositoryMock.Verify(x => x.InsertOrUpdate(expected), Times.Once);
        }
    }
}