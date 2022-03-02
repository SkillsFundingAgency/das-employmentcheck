using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading;
using SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetEmployersPayeSchemesActivityTests
{
    public class WhenCallingGet
    {
        private Fixture _fixture;
        private Mock<IMediator> _mediator;
        private Data.Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
        }

        [Test]
        public void Then_The_EmployerPayeSchemes_Are_Returned()
        {
            // Arrange
            var sut = new GetEmployerPayeSchemesActivity(_mediator.Object);
            var employersPayeSchemes = _fixture.Create<GetPayeSchemesQueryResult>();

            _mediator
                .Setup(x => x.Send(It.IsAny<GetPayeSchemesQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(employersPayeSchemes);

            // Act
            var result = sut.Get(_employmentCheck).Result;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(employersPayeSchemes.EmployersPayeSchemes, result);
        }
    }
}