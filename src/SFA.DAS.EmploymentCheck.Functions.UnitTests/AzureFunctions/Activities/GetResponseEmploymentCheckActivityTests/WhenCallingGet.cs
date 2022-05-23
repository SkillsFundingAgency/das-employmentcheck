using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetResponseEmploymentCheck;
using SFA.DAS.EmploymentCheck.Queries.ResetEmploymentChecksMessageSentDate;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetResponseEmploymentCheckActivityTests
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
            _employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Completed).Create();
        }

        [Test]
        public async Task Then_The_EmploymentChecks_Are_Returned()
        {
            // Arrange
            var sut = new GetResponseEmploymentCheckActivity(_dispatcher.Object);
            var queryResult = new GetResponseEmploymentCheckQueryResult(_employmentCheck);

            _dispatcher
                .Setup(x => x.Send<GetResponseEmploymentCheckQueryRequest, GetResponseEmploymentCheckQueryResult>(
                    It.IsAny<GetResponseEmploymentCheckQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = await sut.Get(null);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.EmploymentCheck, result);
        }
    }
}