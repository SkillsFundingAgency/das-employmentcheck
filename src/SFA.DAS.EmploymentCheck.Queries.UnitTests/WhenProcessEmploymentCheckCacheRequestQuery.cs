using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.ProcessEmploymentCheckCacheRequest;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests
{
    public class WhenProcessEmploymentCheckCacheRequestQuery
    {
        private ProcessEmploymentCheckCacheRequestQueryHandler _sut;
        private Mock<IEmploymentCheckService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<IEmploymentCheckService>(); 
            _sut = new ProcessEmploymentCheckCacheRequestQueryHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_Service_is_called()
        {
            // Arrange
            var request = _fixture.Create<ProcessEmploymentCheckCacheRequestQueryRequest>();
            var expected = _fixture.Create<EmploymentCheckCacheRequest>();
            _serviceMock.Setup(s => s.GetEmploymentCheckCacheRequest()).ReturnsAsync(expected);

            // Act
            var actual = await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.GetEmploymentCheckCacheRequest(), Times.Once);
            actual.EmploymentCheckCacheRequest.Should().BeEquivalentTo(expected);
        }
    }
}
