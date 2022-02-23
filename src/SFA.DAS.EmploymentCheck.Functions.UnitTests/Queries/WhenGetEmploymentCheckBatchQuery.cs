using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentCheck;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Queries
{
    public class WhenGetEmploymentCheckBatchQuery
    {
        private GetEmploymentCheckQueryHandler _sut;
        private Mock<IEmploymentCheckService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<IEmploymentCheckService>();
            _sut = new GetEmploymentCheckQueryHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_Service_is_called()
        {
            // Arrange
            var request = _fixture.Create<GetEmploymentCheckQueryRequest>();
            var expected = _fixture.Create<Models.EmploymentCheck>();

            _serviceMock
                .Setup(s => s.GetEmploymentCheck())
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.GetEmploymentCheck(), Times.Once);
            actual.EmploymentCheck.Should().BeEquivalentTo(expected);
        }
    }
}