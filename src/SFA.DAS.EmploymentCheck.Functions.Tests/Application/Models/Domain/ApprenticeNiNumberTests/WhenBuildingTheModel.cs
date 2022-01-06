using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Models.Domain.ApprenticeNiNumberTests
{
    public class WhenBuildingTheModel
    {
        [Fact]
        public void Then_The_Model_Is_Built_Correctly()
        {
            //Arrange

            var uln = 1000001;
            var niNumber = "ni number";

            //Act

            var result = new LearnerNiNumber(uln, niNumber);

            //Assert

            Assert.Equal(uln, result.Uln);
            Assert.Equal(niNumber, result.NiNumber);
        }
    }
}