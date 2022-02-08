using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetEmploymentChecksBatchActivityTests
{
    public class WhenCallingGet
    {
        private readonly Fixture _fixture;
        private readonly IList<Models.EmploymentCheck> _employmentChecks;
        private readonly Mock<IMediator> _mediator;

        public WhenCallingGet()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _employmentChecks = new List<Models.EmploymentCheck> { _fixture.Create<Models.EmploymentCheck>() };
        }

        [Test]
        public async Task Then_The_EmploymentChecks_Are_Returned()
        {
            //Arrange
            var sut = new GetEmploymentChecksBatchActivity(_mediator.Object);

            var queryResult = new GetEmploymentCheckBatchQueryResult(_employmentChecks);

            _mediator.Setup(x => x.Send(It.IsAny<GetEmploymentCheckBatchQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            //Act
            var result = await sut.Get(null);

            //Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.ApprenticeEmploymentChecks, result);
        }
    }
}