using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Queries.GetEmploymentChecksBatch
{
    public class WhenHandlingTheRequest
    {
        private Fixture _fixture;
        private Mock<IEmploymentCheckClient> _employmentCheckClient;
        private Mock<ILogger<GetEmploymentCheckBatchQueryHandler>> _logger;
        private IList<Models.EmploymentCheck> _employmentChecks;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckClient = new Mock<IEmploymentCheckClient>();
            _logger = new Mock<ILogger<GetEmploymentCheckBatchQueryHandler>>();
            _employmentChecks = new List<Models.EmploymentCheck> { _fixture.Create<Models.EmploymentCheck>() };
        }

        [Test]
        public async Task Then_The_EmploymentCheckClient_Is_Called()
        {
            // Arrange
            _employmentCheckClient.Setup(x => x.GetEmploymentChecksBatch())
                .ReturnsAsync(new List<Models.EmploymentCheck>());

            var sut = new GetEmploymentCheckBatchQueryHandler(_logger.Object, _employmentCheckClient.Object);

            // Act
            await sut.Handle(new GetEmploymentCheckBatchQueryRequest(), CancellationToken.None);

            // Assert
            _employmentCheckClient.Verify(x => x.GetEmploymentChecksBatch(), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_EmploymentCheckClient_Returns_Null_Then_Null_Is_Returned()
        {
            // Arrange
            _employmentCheckClient.Setup(x => x.GetEmploymentChecksBatch())
                .ReturnsAsync((IList<Models.EmploymentCheck>)null);

            var sut = new GetEmploymentCheckBatchQueryHandler(_logger.Object, _employmentCheckClient.Object);

            // Act
            var result = await sut.Handle(new GetEmploymentCheckBatchQueryRequest(), CancellationToken.None);

            // Assert
            result.ApprenticeEmploymentChecks.Should().BeNull();
        }

        [Test]
        public async Task And_The_EmploymentCheckClient_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            // Arrange
            _employmentCheckClient.Setup(x => x.GetEmploymentChecksBatch())
                .ReturnsAsync(_employmentChecks);

            var sut = new GetEmploymentCheckBatchQueryHandler(_logger.Object, _employmentCheckClient.Object);

            // Act
            var result = await sut.Handle(new GetEmploymentCheckBatchQueryRequest(), CancellationToken.None);

            // Assert
            result.ApprenticeEmploymentChecks.Should().BeEquivalentTo(_employmentChecks);
        }
    }
}
