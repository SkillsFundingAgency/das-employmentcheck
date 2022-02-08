using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.SetCacheRequestRelatedRequestsProcessingStatusActivityTests
{
    public class WhenCallingSet
    {
        private readonly Fixture _fixture;
        private readonly Mock<IMediator> _mediator;
        private readonly IList<EmploymentCheckCacheRequest> _employmentCheckCacheRequests;
        private readonly Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> _status;

        public WhenCallingSet()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _employmentCheckCacheRequests = new List<EmploymentCheckCacheRequest> { _fixture.Create<EmploymentCheckCacheRequest>() };
            _status = new Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>(_fixture.Create<EmploymentCheckCacheRequest>(), _fixture.Create<ProcessingCompletionStatus>());
        }

        [Test]
        public async Task Then_The_EmploymentCheckCacheRequests_Are_Returned()
        {
            //Arrange
            var sut = new SetCacheRequestRelatedRequestsProcessingStatusActivity(_mediator.Object);

            var commandResult = new SetCacheRequestRelatedRequestsProcessingStatusCommandResult(_employmentCheckCacheRequests);

            _mediator.Setup(x => x.Send(It.IsAny<SetCacheRequestRelatedRequestsProcessingStatusCommand>(), CancellationToken.None))
                .ReturnsAsync(commandResult);

            //Act
            var result = await sut.SetEmploymentCheckCacheRequestsRelatedRequestsRequestProcessingStatus(_status);

            //Assert
            Assert.NotNull(result);
            Assert.AreEqual(commandResult.EmploymentCheckCacheRequests.Count, result.Count);
            Assert.AreEqual(commandResult.EmploymentCheckCacheRequests, result);
        }
    }
}