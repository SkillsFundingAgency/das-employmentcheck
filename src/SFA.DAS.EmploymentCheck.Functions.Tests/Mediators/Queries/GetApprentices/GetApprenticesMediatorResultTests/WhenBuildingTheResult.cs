using System.Linq;
using AutoFixture;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch;
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
            var apprentices = fixture.CreateMany<Functions.Application.Models.EmploymentCheck>().ToList();

            //Act
            var result = new GetEmploymentCheckBatchQueryResult(apprentices);

            //Assert
            Assert.Equal(apprentices, result.ApprenticeEmploymentChecks);
        }
    }
}