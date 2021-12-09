using AutoFixture;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks;
using System.Linq;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprentices.GetApprenticesMediatorResultTests
{
    public class WhenBuildingTheResult
    {

        [Fact]
        public void Then_The_Result_Is_Built_Correctly()
        {
            //Arrange
            var fixture = new Fixture();
            var apprentices = fixture.CreateMany<ApprenticeEmploymentCheckModel>().ToList();

            //Act
            var result = new GetApprenticeEmploymentChecksQueryResult(apprentices);

            //Assert
            Assert.Equal(apprentices, result.ApprenticeEmploymentChecks);
        }
    }
}