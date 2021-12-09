using AutoFixture;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes;
using System.Linq;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetEmployerPayeSchemes.GetEmployersPayeSchemesMediatorRequestTests
{
    public class WhenBuildingTheRequest
    {
        [Fact]
        public void Then_The_Request_Is_Built_Successfully()
        {
            //Arrange
            var fixture = new Fixture();
            var apprentices = fixture.CreateMany<ApprenticeEmploymentCheckModel>().ToList();

            //Act
            var result = new GetEmployersPayeSchemesMediatorRequest(apprentices);

            //Assert
            Assert.Equal(apprentices, result.Apprentices);
        }
    }
}