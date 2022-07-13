using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Queries.GetResponseEmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.GetResponseEmploymentCheck
{
    public class WhenHandlingTheRequest
    {
        private GetResponseEmploymentCheckQueryHandler _sut;
        private Mock<IEmploymentCheckService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<IEmploymentCheckService>();
            _sut = new GetResponseEmploymentCheckQueryHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_Service_is_called()
        {
            // Arrange
            var request = _fixture.Create<GetResponseEmploymentCheckQueryRequest>();
            var expected = _fixture.Create<Data.Models.EmploymentCheck>();

            _serviceMock
                .Setup(s => s.GetResponseEmploymentCheck())
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.GetResponseEmploymentCheck(), Times.Once);
            actual.EmploymentCheck.Should().BeEquivalentTo(expected);
        }
    }
}