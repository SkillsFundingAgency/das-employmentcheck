using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Queries.GetEmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.GetEmploymentCheck
{
    public class WhenHandlingTheRequest
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
            var expected = _fixture.Create<Data.Models.EmploymentCheck>();

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