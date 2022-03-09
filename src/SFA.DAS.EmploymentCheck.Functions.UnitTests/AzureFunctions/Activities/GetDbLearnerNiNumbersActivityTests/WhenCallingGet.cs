using System;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetDbNiNumber;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetDbLearnerNiNumbersActivityTests
{
    public class WhenCallingGet
    {
        private Fixture _fixture;
        private Mock<IQueryDispatcher> _dispatcher;
        private GetDbLearnerNiNumberActivity _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _dispatcher = new Mock<IQueryDispatcher>();
            _sut = new GetDbLearnerNiNumberActivity(_dispatcher.Object);
        }

        [Test]
        public void Then_The_LearnerNiNumbers_Are_Returned()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var queryResult = new GetDbNiNumberQueryResult(learnerNiNumber);

            _dispatcher
                .Setup(x => x.Send<GetDbNiNumberQueryRequest, GetDbNiNumberQueryResult>(It.IsAny<GetDbNiNumberQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = _sut.Get(employmentCheck).Result;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.LearnerNiNumber, result);
        }
    }
}