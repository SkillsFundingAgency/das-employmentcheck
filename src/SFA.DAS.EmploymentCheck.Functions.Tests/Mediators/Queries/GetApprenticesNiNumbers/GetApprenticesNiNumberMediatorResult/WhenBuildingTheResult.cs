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

            var learnerNiNumber = new LearnerNiNumber(1000001, "1000001");
            var learnerNiNumbers = new List<LearnerNiNumber> {learnerNiNumber};

            //Act

            //var result = new GetLearnerNiNumbersResultQuery(learnerNiNumbers);

            //Assert

            //Assert.Equal(learnerNiNumbers, result.LearnerNiNumber);
        }
    }
}