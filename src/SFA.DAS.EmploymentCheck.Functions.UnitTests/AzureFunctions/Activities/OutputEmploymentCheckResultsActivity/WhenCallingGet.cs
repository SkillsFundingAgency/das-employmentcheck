using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetEmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.OutputEmploymentCheckResultsActivity
{
    public class WhenCallingGet
    {
        private Fixture _fixture;
        private Data.Models.EmploymentCheck _employmentCheck;
        private Mock<IQueryDispatcher> _dispatcher;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _dispatcher = new Mock<IQueryDispatcher>();
            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
        }

        [Test]
        public async Task Then_The_Processed_EmploymentChecks_Are_Returned()
        {
            // Arrange
            var sut = new GetEmploymentCheckActivity(_dispatcher.Object);
            var queryResult = new GetEmploymentCheckQueryResult(_employmentCheck);

            _dispatcher
                .Setup(x => x.Send<GetEmploymentCheckQueryRequest, GetEmploymentCheckQueryResult>(It.IsAny<GetEmploymentCheckQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = await sut.Get(null);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.EmploymentCheck, result);
        }
    }
}