using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentCheck;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetEmploymentCheckActivityTests
{
    public class WhenCallingGet
    {
        private Fixture _fixture;
        private Models.EmploymentCheck _employmentCheck;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _employmentCheck = _fixture.Create<Models.EmploymentCheck>();
        }

        [Test]
        public async Task Then_The_EmploymentChecks_Are_Returned()
        {
            // Arrange
            var sut = new GetEmploymentCheckActivity(_mediator.Object);

            var queryResult = new GetEmploymentCheckQueryResult();
            queryResult.EmploymentCheck = _employmentCheck;

            _mediator
                .Setup(x => x.Send(It.IsAny<GetEmploymentCheckQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            // Act
            var result = await sut.Get(null);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(queryResult.EmploymentCheck, result);
        }
    }
}