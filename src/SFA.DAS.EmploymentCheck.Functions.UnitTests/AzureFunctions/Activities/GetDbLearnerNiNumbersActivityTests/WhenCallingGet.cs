﻿using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumbers;
using System.Linq;
using System.Threading;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetDbLearnerNiNumbersActivityTests
{
    public class WhenCallingGet
    {
        Fixture _fixture;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
        }

        [Test]
        public void Then_The_LearnerNiNumbers_Are_Returned()
        {
            // Arrange
            var employmentCheckBatch = _fixture.CreateMany<Models.EmploymentCheck>(1).ToList();
            var learnerNiNumbers = _fixture.CreateMany<LearnerNiNumber>(1).ToList();
            var sut = new GetDbLearnerNiNumbersActivity(_mediator.Object);
            var queryResult = new GetDbNiNumbersQueryResult(learnerNiNumbers);

            _mediator
                .Setup(x => x.Send(It.IsAny<GetDbNiNumbersQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = sut.Get(employmentCheckBatch).Result;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.LearnerNiNumbers.Count(), result.Count);
            Assert.AreEqual(queryResult.LearnerNiNumbers, result);
        }

        [Test]
        public void Then_If_The_Query_Returns_No_LearnerNiNumbers_Then_An_Empty_List_Is_Returned()
        {
            // Arrange
            var employmentCheckBatch = _fixture.CreateMany<Models.EmploymentCheck>(1).ToList();
            var sut = new GetDbLearnerNiNumbersActivity(_mediator.Object);
            var queryResult = new GetDbNiNumbersQueryResult(null);

            _mediator
                .Setup(x => x.Send(It.IsAny<GetDbNiNumbersQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = sut.Get(employmentCheckBatch).Result;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}