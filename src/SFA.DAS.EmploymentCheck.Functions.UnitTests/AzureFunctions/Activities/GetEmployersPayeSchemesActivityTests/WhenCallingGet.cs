using System.Threading;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.GetEmployersPayeSchemesActivityTests
{
    public class WhenCallingGet
    {
        private Fixture _fixture;
        private Mock<IQueryDispatcher> _dispatcher;
        private Data.Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _dispatcher = new Mock<IQueryDispatcher>();
            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
        }

        [Test]
        public void Then_The_EmployerPayeSchemes_Are_Returned()
        {
            // Arrange
            var sut = new GetEmployerPayeSchemesActivity(_dispatcher.Object);
            var employersPayeSchemes = _fixture.Create<GetPayeSchemesQueryResult>();

            _dispatcher
                .Setup(x => x.Send<GetPayeSchemesQueryRequest, GetPayeSchemesQueryResult>(It.IsAny<GetPayeSchemesQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(employersPayeSchemes);

            // Act
            var result = sut.Get(_employmentCheck).Result;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(employersPayeSchemes.EmployersPayeSchemes, result);
        }
    }
}