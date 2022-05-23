using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheckServiceTests
{
    public class WhenResetEmploymentChecksMessageSentDate
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
                Mock.Of<IUnitOfWork>()
            );
        }

        [Test]
        public async Task The_Repository_ResetEmploymentChecksMessageSentDate_Is_Called_When_Given_A_CorrelationId()
        {
            // Arrange
            var correlationId = Guid.NewGuid();

            _repositoryMock
                .Setup(x => x.ResetEmploymentChecksMessageSentDate(correlationId))
                .Returns(Task.FromResult(1L));

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(correlationId);

            // Assert
            _repositoryMock.Verify(x => x.ResetEmploymentChecksMessageSentDate(correlationId), Times.Once);
            result.Should().Be(1L);
        }

        [Test]
        public async Task The_Repository_ResetEmploymentChecksMessageSentDate_Is_Called()
        {
            // Arrange
            var messageSentFromDate = new DateTime(2022, 3, 23);
            var messageSentToDate = new DateTime(2022, 3, 26);

            _repositoryMock
                .Setup(x => x.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate))
                .Returns(Task.FromResult(1L));

            // Act
            var result = await _sut.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate);

            // Assert
            _repositoryMock.Verify(x => x.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate), Times.Once);
            result.Should().Be(1L);
        }
    }
}