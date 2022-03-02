using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Linq;
using System.Threading;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetDbNiNumber;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetDbLearnerNiNumbersActivityTests
{
    public class WhenCallingGet
    {
        Fixture _fixture;
        private Mock<ILogger<GetDbLearnerNiNumberActivity>> _logger;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _logger = new Mock<ILogger<GetDbLearnerNiNumberActivity>>();
            _mediator = new Mock<IMediator>();
        }

        [Test]
        public void Then_The_LearnerNiNumbers_Are_Returned()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var sut = new GetDbLearnerNiNumberActivity(_mediator.Object);
            var queryResult = new GetDbNiNumberQueryResult(learnerNiNumber);

            _mediator
                .Setup(x => x.Send(It.IsAny<GetDbNiNumberQueryRequest>(), CancellationToken.None))
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
            var sut = new GetDbLearnerNiNumberActivity(_mediator.Object);
            var queryResult = new GetDbNiNumberQueryResult(null);

            _mediator
                .Setup(x => x.Send(It.IsAny<GetDbNiNumberQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = sut.Get(employmentCheck).Result;

            // Assert
            Assert.NotNull(result);
        }
    }
}