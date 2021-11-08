using System;
using System.Collections.Generic;
using System.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetEmployersPayeSchemesActivityTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetEmployersPayeSchemesActivity>> _logger;
        private readonly Apprentice _apprentice;
        private readonly IList<Apprentice> _apprentices;
        public WhenCallingGet()
        {
            _mediator = new Mock<IMediator>();

            _logger = new Mock<ILogger<GetEmployersPayeSchemesActivity>>();

            _apprentice = new Apprentice(1,
                1000001,
                "1000001",
                1000001,
                1000001,
                1000001,
                DateTime.Today,
                DateTime.Today.AddDays(1));

            _apprentices = new List<Apprentice> { _apprentice };
        }

        [Fact]
        public void Then_The_EmployerPayeSchemes_Are_Returned()
        {
            //Arrange
            var sut = new GetEmployersPayeSchemesActivity(_mediator.Object, _logger.Object);

            var employersPayeSchemes = new GetEmployersPayeSchemesMediatorResult(new List<EmployerPayeSchemes> { new EmployerPayeSchemes(1, new List<string>()) });

            _mediator.Setup(x => x.Send(It.IsAny<GetEmployersPayeSchemesMediatorRequest>(), CancellationToken.None))
                .ReturnsAsync(employersPayeSchemes);

            //Act
            var result = sut.Get(_apprentices).Result;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(employersPayeSchemes.EmployersPayeSchemes.Count, result.Count);
            Assert.Equal(employersPayeSchemes.EmployersPayeSchemes, result);
        }
        [Fact]
        public void And_Throws_An_Expection_Then_Exception_Is_Handled()
        {
            //Arrange
            var exception = new Exception("test message");
            var sut = new GetEmployersPayeSchemesActivity(_mediator.Object, _logger.Object);

            _mediator.Setup(x => x.Send(It.IsAny<GetEmployersPayeSchemesMediatorRequest>(), CancellationToken.None))
                .ThrowsAsync(exception);

            //Act

            var result = sut.Get(_apprentices).Result;

            //Assert
            Assert.Equal(new List<EmployerPayeSchemes>(), result);
        }
    }
}