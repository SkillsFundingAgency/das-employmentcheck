using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheckServiceTests
{
    public class WhenGettingResponseEmploymentCheck
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
            await _sut.GetResponseEmploymentCheck();

            // Assert
            _repositoryMock.Verify(x => x.GetResponseEmploymentCheck(), Times.AtLeastOnce());
        }

        [Test]
        public async Task And_The_Repository_Returns_Processed_EmploymentCheck_Then_Is_Is_Returned()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Completed)
                .Without(x => x.MessageSentDate)
                .Create();

            _repositoryMock.Setup(x => x.GetResponseEmploymentCheck())
                .ReturnsAsync(employmentCheck);

            // Act
            var result = await _sut.GetResponseEmploymentCheck();

            // Assert
            Assert.AreEqual(employmentCheck, result);
        }
    }
}