using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetHmrcLearnerEmploymentStatus;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetHmrcLearnerEmploymentStatusActivityTests
{
    public class WhenCallingGet
    {
        private Fixture _fixture;
        private Mock<IQueryDispatcher> _dispatcher;
        private EmploymentCheckCacheRequest _employmentCheckCacheRequest;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _dispatcher = new Mock<IQueryDispatcher>();
            _employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
        }

        [Test]
        public async Task Then_The_LearnerEmploymentStatus_Is_Returned()
        {
            // Arrange
            var sut = new GetHmrcLearnerEmploymentStatusActivity(_dispatcher.Object);

            var queryResult = new GetHmrcLearnerEmploymentStatusQueryResult(_employmentCheckCacheRequest);

            _dispatcher
                .Setup(x => x.Send<GetHmrcLearnerEmploymentStatusQueryRequest, GetHmrcLearnerEmploymentStatusQueryResult>(It.IsAny<GetHmrcLearnerEmploymentStatusQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = await sut.GetHmrcEmploymentStatusTask(null);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.EmploymentCheckCacheRequest, result);
        }
    }
}