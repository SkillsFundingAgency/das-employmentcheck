using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.ProcessEmploymentCheckCacheRequest;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetEmploymentCheckCacheRequestActivityTests
{
    public class WhenCallingGet
    {
        private Fixture _fixture;
        private EmploymentCheckCacheRequest _request;
        private Mock<IQueryDispatcher> _dispatcher;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _dispatcher = new Mock<IQueryDispatcher>();
            _request = _fixture.Create<EmploymentCheckCacheRequest>();
        }

        [Test]
        public async Task Then_The_EmploymentCheckCacheRequest_Is_Returned()
        {
            // Arrange
            var sut = new GetEmploymentCheckCacheRequestActivity(_dispatcher.Object);

            var queryResult = new ProcessEmploymentCheckCacheRequestQueryResult(_request);

            _dispatcher
                .Setup(x => x.Send<ProcessEmploymentCheckCacheRequestQueryRequest, ProcessEmploymentCheckCacheRequestQueryResult>(It.IsAny<ProcessEmploymentCheckCacheRequestQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = await sut.GetEmploymentCheckRequestActivityTask(null);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.EmploymentCheckCacheRequest, result);
        }
    }
}