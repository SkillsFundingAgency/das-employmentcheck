using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Queries
{
    public class WhenGetEmploymentCheckBatchQuery
    {
        private GetEmploymentCheckBatchQueryHandler _sut;
        private Mock<IEmploymentCheckService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<IEmploymentCheckService>(); 
            _sut = new GetEmploymentCheckBatchQueryHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_Service_is_called()
        {
            // Arrange
            var request = _fixture.Create<GetEmploymentCheckBatchQueryRequest>();
            var expected = _fixture.CreateMany<Functions.Application.Models.EmploymentCheck>().ToList();
            _serviceMock.Setup(s => s.GetEmploymentChecksBatch()).ReturnsAsync(expected);

            // Act
            var actual = await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.GetEmploymentChecksBatch(), Times.Once);
            actual.ApprenticeEmploymentChecks.Should().BeEquivalentTo(expected);
        }
    }
}
