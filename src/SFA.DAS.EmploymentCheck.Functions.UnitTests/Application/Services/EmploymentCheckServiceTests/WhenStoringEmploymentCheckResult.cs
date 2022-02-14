//using AutoFixture;
//using FluentAssertions;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NUnit.Framework;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models;
//using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
//using SFA.DAS.EmploymentCheck.Functions.Repositories;
//using System.Threading.Tasks;

//namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Services.EmploymentCheckServiceTests
//{
//    public class WhenStoringEmploymentCheckResult
//    {
//        private Fixture _fixture;

//        private Mock<ILogger<IEmploymentCheckService>> _logger;
//        private Mock<IEmploymentCheckRepository> _employmentCheckRepository;
//        private Mock<IEmploymentCheckCacheRequestRepository> _employmentCheckCashRequestRepository;
//        private IEmploymentCheckService _sut;

//        [SetUp]
//        public void SetUp()
//        {
//            _fixture = new Fixture();

//            _logger = new Mock<ILogger<IEmploymentCheckService>>(MockBehavior.Strict);
//            _employmentCheckRepository = new Mock<IEmploymentCheckRepository>(MockBehavior.Strict);
//            _employmentCheckCashRequestRepository = new Mock<IEmploymentCheckCacheRequestRepository>(MockBehavior.Strict);

//            _sut = new EmploymentCheckService(
//                _logger.Object,
//                _employmentCheckRepository.Object,
//                _employmentCheckCashRequestRepository.Object);
//        }

//        [Test]
//        public async Task Then_EmploymentCheckCacheRequest_RequestProcessingStatus_Is_Stored()
//        {
//            // Arrange
//            var employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();

//            _employmentCheckCashRequestRepository.Setup(r => r.UpdateEmployedAndRequestStatusFields(employmentCheckCacheRequest))
//                .Returns(Task.FromResult(0));

//            // Act
//            await _sut.StoreEmploymentCheckResult(employmentCheckCacheRequest);

//            // Assert
//            _employmentCheckCashRequestRepository.Verify(r => r.UpdateEmployedAndRequestStatusFields(employmentCheckCacheRequest), Times.Exactly(1));
//        }

//        [Test]
//        public async Task And_The_EmploymentCheck_RequestProcessingStatus_Is_Stored()
//        {
//            // Arrange
//            var employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();

//            _employmentCheckCashRequestRepository.Setup(r => r.UpdateEmployedAndRequestStatusFields(employmentCheckCacheRequest))
//                .Returns(Task.CompletedTask);

//            _employmentCheckRepository.Setup(r => r.UpdateEmployedAndRequestStatusFields(employmentCheckCacheRequest))
//                .Returns(Task.FromResult(employmentCheckCacheRequest.Id));

//            // Act
//            var result = await _sut.StoreEmploymentCheckResult(employmentCheckCacheRequest);

//            // Assert
//            _employmentCheckCashRequestRepository.Verify(r => r.UpdateEmployedAndRequestStatusFields(employmentCheckCacheRequest), Times.Exactly(1));
//            _employmentCheckRepository.Verify(r => r.UpdateEmployedAndRequestStatusFields(employmentCheckCacheRequest), Times.Exactly(1));
//            result.Should().Be(employmentCheckCacheRequest.Id);
//        }
//    }
//}