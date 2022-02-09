using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Queries.GetApprentices
{
    public class WhenBuildingTheResult
    {

        [Test]
        public void Then_The_Result_Is_Built_Correctly()
        {
            //Arrange
            var fixture = new Fixture();
            var apprentices = fixture.CreateMany<Functions.Application.Models.EmploymentCheck>().ToList();

            //Act
            var result = new GetEmploymentCheckBatchQueryResult(apprentices);

            //Assert
            Assert.AreEqual(apprentices, result.ApprenticeEmploymentChecks);
        }
    }
}