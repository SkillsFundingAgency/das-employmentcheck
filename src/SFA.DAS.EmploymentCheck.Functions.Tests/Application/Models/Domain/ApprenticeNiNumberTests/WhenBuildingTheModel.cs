﻿using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
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

            var result = new ApprenticeNiNumber(uln, niNumber);

            //Assert
            
            Assert.Equal(uln, result.ULN);
            Assert.Equal(niNumber, result.NationalInsuranceNumber);
        }
    }
}