using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheckClientTests
{
    public class WhenGettingEmploymentCheckCacheRequest
    {
        private readonly Mock<IEmploymentCheckService> _employmentCheckServiceMock = new Mock<IEmploymentCheckService>();
        private EmploymentCheckClient _sut;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _sut = new EmploymentCheckClient(_employmentCheckServiceMock.Object);
        }

        [Test]
        public async Task Then_The_EmploymentCheckService_Is_Called()
        {
            // Arrange

            // Act
            await _sut.GetEmploymentCheckCacheRequest();

            // Assert
            _employmentCheckServiceMock.Verify(x => x.GetEmploymentCheckCacheRequest(), Times.AtLeastOnce);
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_An_EmploymentCheckCacheRequest_Then_It_Is_Returned()
        {
            // Arrange
            var employmentCheckCacheRequest = _fixture
                .Build<EmploymentCheckCacheRequest>()
                .Create();

            _employmentCheckServiceMock
                .Setup(x => x.GetEmploymentCheckCacheRequest())
                .ReturnsAsync(employmentCheckCacheRequest);

            // Act
            var result = await _sut.GetEmploymentCheckCacheRequest();

            // Assert
            Assert.AreEqual(employmentCheckCacheRequest, result);
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_Null_Then_Null_Is_Returned()
        {
            // Arrange
            _employmentCheckServiceMock
                .Setup(x => x.GetEmploymentCheckCacheRequest())
                .ReturnsAsync(() => null);

            // Act
            var result = await _sut.GetEmploymentCheckCacheRequest();

            // Assert
            result.Should().BeNull();
        }
    }
}