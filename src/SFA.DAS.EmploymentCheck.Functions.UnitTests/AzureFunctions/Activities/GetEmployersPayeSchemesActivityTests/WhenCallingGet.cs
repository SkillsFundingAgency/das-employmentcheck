using System;
using System.Collections.Generic;
using System.Threading;
using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetEmployersPayeSchemesActivityTests
{
    public class WhenCallingGet
    {
        Fixture _fixture;
        private Mock<IMediator> _mediator;
        private Mock<ILogger<GetEmployerPayeSchemesActivity>> _logger;
        private IList<Models.EmploymentCheck> _apprentices;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GetEmployerPayeSchemesActivity>>();

            _apprentices = new List<Models.EmploymentCheck>
            {
                _fixture
                .Create<Models.EmploymentCheck>()
            };
        }

        [Test]
        public void Then_The_EmployerPayeSchemes_Are_Returned()
        {
            //Arrange
            var sut = new GetEmployerPayeSchemesActivity(_mediator.Object, _logger.Object);

            var payes = new List<EmployerPayeSchemes>
            {
                _fixture
                .Build<EmployerPayeSchemes>()
                .Create()
            };

            var employersPayeSchemes = new GetPayeSchemesQueryResult(payes);

            _mediator
                .Setup(x => x.Send(It.IsAny<GetPayeSchemesQueryRequest>(), CancellationToken.None))
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