using AutoFixture;
using MediatR;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetEmploymentChecksBatchActivityTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Fixture _fixture;

        public WhenCallingGet()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
        }

        [Test]
        public void Then_Learners_Are_Returned()
        {
            //Arrange
            var apprentices = new List<Functions.Application.Models.EmploymentCheck> { _fixture.Create<Functions.Application.Models.EmploymentCheck>() };
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