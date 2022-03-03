using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetDbNiNumber;
using System.Threading;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetDbLearnerNiNumbersActivityTests
{
    public class WhenCallingGet
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
        public void Then_The_LearnerNiNumbers_Are_Returned()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var sut = new GetDbLearnerNiNumberActivity(_dispatcher.Object);
            var queryResult = new GetDbNiNumberQueryResult(learnerNiNumber);

            _dispatcher
                .Setup(x => x.Send<GetDbNiNumberQueryRequest, GetDbNiNumberQueryResult>(It.IsAny<GetDbNiNumberQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = sut.Get(employmentCheck).Result;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.LearnerNiNumber, result);
        }

        [Test]
        public void Then_If_The_Query_Returns_No_LearnerNiNumbers_Then_An_Empty_LearnerNiNumber_Is_Returned()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            var sut = new GetDbLearnerNiNumberActivity(_dispatcher.Object);
            var queryResult = new GetDbNiNumberQueryResult(null);

            _dispatcher
                .Setup(x => x.Send<GetDbNiNumberQueryRequest, GetDbNiNumberQueryResult>(It.IsAny<GetDbNiNumberQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = sut.Get(employmentCheck).Result;

            // Assert
            Assert.NotNull(result);
        }
    }
}