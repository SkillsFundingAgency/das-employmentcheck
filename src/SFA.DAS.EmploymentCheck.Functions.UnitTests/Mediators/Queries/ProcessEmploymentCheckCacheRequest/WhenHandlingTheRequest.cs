using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.ProcessEmploymentCheckCacheRequest;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Queries.ProcessEmploymentCheckCacheRequest
{
    public class WhenHandlingTheRequest
    {
        private Fixture _fixture;
        private Mock<IEmploymentCheckClient> _employmentCheckClient;
        private EmploymentCheckCacheRequest _employmentCheckCacheRequest;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckClient = new Mock<IEmploymentCheckClient>();
            _employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
        }

        [Test]
        public async Task Then_The_EmploymentCheckClient_Is_Called()
        {
            // Arrange
            _employmentCheckClient.Setup(x => x.GetEmploymentCheckCacheRequest())
                .ReturnsAsync(new EmploymentCheckCacheRequest());

            var sut = new ProcessEmploymentCheckCacheRequestQueryHandler(_employmentCheckClient.Object);

            // Act
            await sut.Handle(new ProcessEmploymentCheckCacheRequestQueryRequest(), CancellationToken.None);

            // Assert
            _employmentCheckClient.Verify(x => x.GetEmploymentCheckCacheRequest(), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_EmploymentCheckClient_Returns_Null_Then_Null_Is_Returned()
        {
            // Arrange
            _employmentCheckClient.Setup(x => x.GetEmploymentCheckCacheRequest())
                .ReturnsAsync((EmploymentCheckCacheRequest)null);

            var sut = new ProcessEmploymentCheckCacheRequestQueryHandler(_employmentCheckClient.Object);

            // Act
            var result = await sut.Handle(new ProcessEmploymentCheckCacheRequestQueryRequest(), CancellationToken.None);

            // Assert
            result.EmploymentCheckCacheRequest.Should().BeNull();
        }

        [Test]
        public async Task And_The_EmploymentCheckClient_Returns_EmploymentCheckCacheRequest_Then_They_Are_Returned()
        {
            _employmentCheckClient.Setup(x => x.GetEmploymentCheckCacheRequest())
                .ReturnsAsync(_employmentCheckCacheRequest);

            var sut = new ProcessEmploymentCheckCacheRequestQueryHandler(_employmentCheckClient.Object);

            // Act
            var result = await sut.Handle(new ProcessEmploymentCheckCacheRequestQueryRequest(), CancellationToken.None);

            //Assert

            result.EmploymentCheckCacheRequest.Should().BeEquivalentTo(_employmentCheckCacheRequest);
        }
    }
}