using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprenticesNiNumbers.GetApprenticesNiNumberMediatorResult
{
    public class WhenBuildingTheResult
    {
        [Fact]
        public void Then_The_Result_Is_Built_Successfully()
        {
            //Arrange

            var apprenticeNiNumber = new ApprenticeNiNumber(1000001, "1000001");
            var apprenticeNiNumbers = new List<ApprenticeNiNumber> {apprenticeNiNumber};

            //Act

            var result = new Functions.Mediators.Queries.GetApprenticesNiNumbers.GetApprenticesNiNumberMediatorResult(apprenticeNiNumbers);

            //Assert

            Assert.Equal(apprenticeNiNumbers, result.ApprenticesNiNumber);
        }
    }
}