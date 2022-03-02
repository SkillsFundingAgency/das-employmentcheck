using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetEmployerPayeSchemes.GetEmployersPayeSchemesMediatorResultTests
{

    public class WhenBuildingTheResult
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_The_Result_Is_Built_Successfully()
        {
            // Arrange
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();

            // Act
            var result = new GetPayeSchemesQueryResult(payeScheme);

            // Assert
            Assert.AreEqual(payeScheme, result.EmployersPayeSchemes);
        }
    }
}