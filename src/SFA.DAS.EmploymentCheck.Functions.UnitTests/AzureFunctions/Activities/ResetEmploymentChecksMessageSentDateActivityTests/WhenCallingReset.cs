using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.ResetEmploymentChecksMessageSentDate;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.ResetEmploymentChecksMessageSentDateActivityTests
{
    public class WhenCallingReset
    {
        private Fixture _fixture;
        private Mock<IQueryDispatcher> _dispatcher;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _dispatcher = new Mock<IQueryDispatcher>();
        }

        [Test]
        public async Task Then_The_Query_Was_Executed_With_CorrelationId()
        {
            // Arrange
            var employmentCheckMessageSentData = "CorrelationId=EE9DF079-0F35-45F1-8451-B05433A738C5";
            var updatedRowsCount = 1;
            var queryResult = new ResetEmploymentChecksMessageSentDateQueryResult(updatedRowsCount);
            var sut = new ResetEmploymentChecksMessageSentDateActivity(_dispatcher.Object);

            _dispatcher
                .Setup(x => x.Send<ResetEmploymentChecksMessageSentDateQueryRequest, ResetEmploymentChecksMessageSentDateQueryResult>(
                    It.IsAny<ResetEmploymentChecksMessageSentDateQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = await sut.Reset(employmentCheckMessageSentData);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.UpdatedRowsCount, result);
        }

        [Test]
        public async Task Then_The_Query_Was_Executed_With_Dates()
        {
            // Arrange
            var employmentCheckMessageSentData = "MessageSentFromDate=2022-03-23&MessageSentToDate=2022-03-25";
            var updatedRowsCount = 1;
            var queryResult = new ResetEmploymentChecksMessageSentDateQueryResult(updatedRowsCount);
            var sut = new ResetEmploymentChecksMessageSentDateActivity(_dispatcher.Object);

            _dispatcher
                .Setup(x => x.Send<ResetEmploymentChecksMessageSentDateQueryRequest, ResetEmploymentChecksMessageSentDateQueryResult>(
                    It.IsAny<ResetEmploymentChecksMessageSentDateQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = await sut.Reset(employmentCheckMessageSentData);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.UpdatedRowsCount, result);
        }
    }
}