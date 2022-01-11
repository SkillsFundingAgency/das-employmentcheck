using System.Collections.Generic;
using System.Threading;
using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Activities;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetPayeSchemes;
using SFA.DAS.EmploymentCheck.Application.Common.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetEmployersPayeSchemesActivityTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetEmployerPayeSchemesActivity>> _logger;
        private readonly IList<Domain.Entities.EmploymentCheck> _apprentices;

        public WhenCallingGet()
        {
            var fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GetEmployerPayeSchemesActivity>>();
            _apprentices = new List<Domain.Entities.EmploymentCheck>
                {fixture.Create<Domain.Entities.EmploymentCheck>()};
        }

        [Test]
        public void Then_The_EmployerPayeSchemes_Are_Returned()
        {
            //Arrange
            var sut = new GetEmployerPayeSchemesActivity(_mediator.Object);

            var employersPayeSchemes = new GetPayeSchemesQueryResult(new List<EmployerPayeSchemes>
                {new EmployerPayeSchemes(1, new List<string>())});

            _mediator.Setup(x => x.Send(It.IsAny<GetPayeSchemesQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(employersPayeSchemes);

            //Act
            var result = sut.Get(_apprentices).Result;

            //Assert
            Assert.NotNull(result);
            Assert.AreEqual(employersPayeSchemes.EmployersPayeSchemes.Count, result.Count);
            Assert.AreEqual(employersPayeSchemes.EmployersPayeSchemes, result);
        }
    }
}