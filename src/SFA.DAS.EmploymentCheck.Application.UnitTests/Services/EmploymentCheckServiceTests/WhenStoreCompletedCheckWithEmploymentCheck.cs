using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using Models = SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheck.EmploymentCheckTests
{
    public class WhenStoreCompletedCheckWithEmploymentCheck
    {
        private IEmploymentCheckService _sut;
        private Fixture _fixture;
        private Mock<IEmploymentCheckRepository> _employmentCheckRepositoryMock;
        private Mock<IEmploymentCheckCacheRequestRepository> _employmentCheckCacheRequestRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckRepositoryMock = new Mock<IEmploymentCheckRepository>();
            _employmentCheckCacheRequestRepositoryMock = new Mock<IEmploymentCheckCacheRequestRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _sut = new EmploymentCheckService(
                _employmentCheckRepositoryMock.Object,
                _employmentCheckCacheRequestRepositoryMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Test]
        public async Task Then_The_EmploymentCheck_is_Stored()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Models.EmploymentCheck>();

            _employmentCheckRepositoryMock.Setup(r => r.InsertOrUpdate(employmentCheck))
                .Returns(Task.FromResult(0));

            // Act
            await _sut.StoreCompletedEmploymentCheck(employmentCheck);

            // Assert
            _employmentCheckRepositoryMock.Verify(r => r.InsertOrUpdate(employmentCheck), Times.Once);
            employmentCheck.RequestCompletionStatus.Should().Be((short)ProcessingCompletionStatus.Completed);
        }
    }
}
