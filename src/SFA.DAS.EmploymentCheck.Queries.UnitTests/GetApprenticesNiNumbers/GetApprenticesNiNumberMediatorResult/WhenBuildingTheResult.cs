using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetNiNumbers;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.GetApprenticesNiNumbers.GetApprenticesNiNumberMediatorResult
{
    public class WhenBuildingTheResult
    {
        [Test]
        public void Then_The_Result_Is_Built_Successfully()
        {
            //Arrange

            var apprenticeNiNumber = new LearnerNiNumber(1000001, "1000001");
            var apprenticeNiNumbers = new List<LearnerNiNumber> {apprenticeNiNumber};

            //Act

            var result = new GetNiNumbersQueryResult(apprenticeNiNumbers);

            //Assert

            Assert.AreEqual(apprenticeNiNumbers, result.LearnerNiNumber);
        }
    }
}