using System;
using System.Collections.Generic;
using System.Threading;
using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetEmployersPayeSchemesActivityTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetEmployerPayeSchemesActivity>> _logger;
        private readonly IList<Functions.Application.Models.EmploymentCheck> _apprentices;

        public WhenCallingGet()
        {
            var fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GetEmployerPayeSchemesActivity>>();
            _apprentices = new List<Functions.Application.Models.EmploymentCheck>
                {fixture.Create<Functions.Application.Models.EmploymentCheck>()};
        }

        [Fact]
        public void Then_The_EmployerPayeSchemes_Are_Returned()
        {
            //Arrange
            var sut = new GetEmployerPayeSchemesActivity(_mediator.Object, _logger.Object);

            var employersPayeSchemes = new GetPayeSchemesQueryResult(new List<EmployerPayeSchemes>
                {new EmployerPayeSchemes(1, new List<string>())});

            _mediator.Setup(x => x.Send(It.IsAny<GetPayeSchemesQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(employersPayeSchemes);

            //Act
            var result = sut.Get(_apprentices).Result;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(employersPayeSchemes.EmployersPayeSchemes.Count, result.Count);
            Assert.Equal(employersPayeSchemes.EmployersPayeSchemes, result);
        }
    }
}