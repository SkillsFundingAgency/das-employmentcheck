﻿using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetHmrcLearnerEmploymentStatus;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetHmrcLearnerEmploymentStatusActivityTests
{
    public class WhenCallingGet
    {
        private readonly Fixture _fixture;
        private readonly IList<Models.EmploymentCheck> _employmentChecks;
        private readonly EmploymentCheckCacheRequest employmentCheckCacheRequest;
        private readonly IList<EmploymentCheckCacheRequest> _request;
        private readonly Mock<ILogger<GetHmrcLearnerEmploymentStatusActivity>> _logger;
        private readonly Mock<IMediator> _mediator;

        public WhenCallingGet()
        {
            _fixture = new Fixture();
            _logger = new Mock<ILogger<GetHmrcLearnerEmploymentStatusActivity>>();
            _mediator = new Mock<IMediator>();
            _employmentChecks = new List<Models.EmploymentCheck> { _fixture.Create<Models.EmploymentCheck>()  };
            employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
            _request = new List<EmploymentCheckCacheRequest> { _fixture.Create<EmploymentCheckCacheRequest>() };
        }

        [Test]
        public async Task Then_The_LearnerEmploymentStatus_Is_Returned()
        {
            //Arrange
            var sut = new GetHmrcLearnerEmploymentStatusActivity(_logger.Object, _mediator.Object);

            var queryResult = new GetHmrcLearnerEmploymentStatusQueryResult(employmentCheckCacheRequest);

            _mediator.Setup(x => x.Send(It.IsAny<GetHmrcLearnerEmploymentStatusQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            //Act
            var result = await sut.GetHmrcEmploymentStatusTask(null);

            //Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.EmploymentCheckCacheRequest, result);
        }
    }
}