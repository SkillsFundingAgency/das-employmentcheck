using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheckClientTests
{
    public class WhenStoringEmploymentCheckResult
    {
        private readonly Mock<IEmploymentCheckService> _employmentCheckServiceMock = new Mock<IEmploymentCheckService>();
        private Fixture _fixture;
        private EmploymentCheckClient _sut;

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
            var employmentCheckCacheRequest = _fixture
                .Build<EmploymentCheckCacheRequest>()
                .Create();

            _employmentCheckServiceMock
                .Setup(x => x.StoreEmploymentCheckResult(employmentCheckCacheRequest));

            // Act
            await _sut.StoreEmploymentCheckResult(employmentCheckCacheRequest);

            // Assert
            _employmentCheckServiceMock.Verify(x => x.StoreEmploymentCheckResult(employmentCheckCacheRequest), Times.AtLeastOnce);
        }
    }
}