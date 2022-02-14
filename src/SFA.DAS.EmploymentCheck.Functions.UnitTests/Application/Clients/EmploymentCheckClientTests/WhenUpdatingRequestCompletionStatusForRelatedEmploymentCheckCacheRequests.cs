//using AutoFixture;
//using Moq;
//using NUnit.Framework;
//using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models;
//using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
//using System.Threading.Tasks;

//namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheckClientTests
//{
//    public class WhenUpdatingRequestCompletionStatusForRelatedEmploymentCheckCacheRequests
//    {

//        private readonly Fixture _fixture;
//        private readonly Mock<IEmploymentCheckService> _employmentCheckServiceMock = new Mock<IEmploymentCheckService>();
//        private readonly EmploymentCheckClient _sut;

//        public WhenUpdatingRequestCompletionStatusForRelatedEmploymentCheckCacheRequests()
//        {
//            _fixture = new Fixture();
//            _sut = new EmploymentCheckClient(_employmentCheckServiceMock.Object);
//        }

//        [Test]
//        public async Task Then_The_EmploymentCheckService_Is_Called()
//        {
//            // Arrange
//            var employmentCheckCacheRequest = _fixture
//                .Build<EmploymentCheckCacheRequest>()
//                .Create();

//            _employmentCheckServiceMock
//                .Setup(x => x.UpdateRelatedRequests(employmentCheckCacheRequest));

//            // Act
//            await _sut
//                .UpdateRequestCompletionStatusForRelatedEmploymentCheckCacheRequests(employmentCheckCacheRequest);

//            // Assert
//            _employmentCheckServiceMock
//                .Verify(x => x.UpdateRelatedRequests(employmentCheckCacheRequest), Times.AtLeastOnce);
//        }
//    }
//}