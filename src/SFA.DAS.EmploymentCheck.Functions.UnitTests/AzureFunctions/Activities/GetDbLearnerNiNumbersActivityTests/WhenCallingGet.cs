using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumber;
using System.Linq;
using System.Threading;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetDbLearnerNiNumbersActivityTests
{
    public class WhenCallingGet
    {
        Fixture _fixture;
        private Mock<ILogger<GetDbLearnerNiNumbersActivity>> _logger;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _logger = new Mock<ILogger<GetDbLearnerNiNumbersActivity>>();
            _mediator = new Mock<IMediator>();
        }

        [Test]
        public void Then_The_LearnerNiNumbers_Are_Returned()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Models.EmploymentCheck>();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var sut = new GetDbLearnerNiNumbersActivity(_logger.Object, _mediator.Object);
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
            var employmentCheck = _fixture.Create<Models.EmploymentCheck>();
            var sut = new GetDbLearnerNiNumbersActivity(_logger.Object, _mediator.Object);
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