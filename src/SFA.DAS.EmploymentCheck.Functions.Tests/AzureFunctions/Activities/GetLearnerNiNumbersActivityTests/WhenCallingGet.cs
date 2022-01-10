﻿using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Activities;
using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetNiNumbers;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetLearnerNiNumbersActivityTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetLearnerNiNumbersActivity>> _logger;
        private readonly LearnerNiNumber _apprenticeNiNumber;
        private readonly IList<Domain.Entities.EmploymentCheck> _apprentices;

        public WhenCallingGet()
        {
            var fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GetLearnerNiNumbersActivity>>();
            _apprenticeNiNumber = fixture.Create<LearnerNiNumber>();
            _apprentices = new List<Domain.Entities.EmploymentCheck> { fixture.Create<Domain.Entities.EmploymentCheck>() };
        }

        [Test]
        public void Then_The_NINumbers_Are_Returned()
        {
            //Arrange
            var sut = new GetLearnerNiNumbersActivity(_mediator.Object);

            var apprenticeNiNumbers = new GetNiNumbersQueryResult(new List<LearnerNiNumber> { _apprenticeNiNumber });

            _mediator.Setup(x => x.Send(It.IsAny<GetNiNumbersQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(apprenticeNiNumbers);

            //Act
            var result = sut.Get(_apprentices).Result;

            //Assert
            Assert.NotNull(result);
            Assert.AreEqual(apprenticeNiNumbers.LearnerNiNumber.Count, result.Count);
            Assert.AreEqual(apprenticeNiNumbers.LearnerNiNumber, result);
        }
        [Test]
        public void And_Throws_An_Exception_Then_Exception_Is_Handled()
        {
            //Arrange
            var exception = new Exception("test message");
            var sut = new GetLearnerNiNumbersActivity(_mediator.Object);

            _mediator.Setup(x => x.Send(It.IsAny<GetNiNumbersQueryRequest>(), CancellationToken.None))
                .ThrowsAsync(exception);

            //Act
            var result = sut.Get(_apprentices).Result;

            //Assert
            Assert.AreEqual(new List<LearnerNiNumber>(), result);
        }
    }
}