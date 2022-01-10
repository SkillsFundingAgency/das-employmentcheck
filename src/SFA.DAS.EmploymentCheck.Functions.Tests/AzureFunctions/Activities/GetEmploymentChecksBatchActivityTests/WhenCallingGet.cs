using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Activities;
using SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetEmploymentChecksBatch;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetEmploymentChecksBatchActivityTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetEmploymentChecksBatchActivity>> _logger;
        private readonly Fixture _fixture;

        public WhenCallingGet()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GetEmploymentChecksBatchActivity>>();
        }

        [Test]
        public void Then_Learners_Are_Returned()
        {
            //Arrange
            var apprentices = new List<Domain.Entities.EmploymentCheck> { _fixture.Create<Domain.Entities.EmploymentCheck>() };
            var sut = new GetEmploymentChecksBatchActivity(_mediator.Object);

            _mediator.Setup(x => x.Send(It.IsAny<GetEmploymentCheckBatchQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetEmploymentCheckBatchQueryResult(apprentices));

            //Act

            var result = sut.Get(_fixture.Create<long>()).Result;

            //Assert
            Assert.NotNull(result);
            Assert.AreEqual(apprentices.Count, result.Count);
            Assert.AreEqual(apprentices, result);
        }
    }
}