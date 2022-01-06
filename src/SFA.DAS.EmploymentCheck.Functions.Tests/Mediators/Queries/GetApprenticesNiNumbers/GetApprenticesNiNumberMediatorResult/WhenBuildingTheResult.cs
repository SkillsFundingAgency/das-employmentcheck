using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprenticesNiNumbers.GetApprenticesNiNumberMediatorResult
{
    public class WhenBuildingTheResult
    {
        [Fact]
        public void Then_The_Result_Is_Built_Successfully()
        {
            //Arrange

            var apprenticeNiNumber = new LearnerNiNumber(1000001, "1000001");
            var apprenticeNiNumbers = new List<LearnerNiNumber> {apprenticeNiNumber};

            //Act

            var result = new Functions.Mediators.Queries.GetNiNumbers.GetNiNumbersQueryResult(apprenticeNiNumbers);

            //Assert

            Assert.Equal(apprenticeNiNumbers, result.LearnerNiNumber);
        }
    }
}