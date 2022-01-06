using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Models.Domain.EmployerPayeSchemesTests
{
    public class WhenBuildingTheModel
    {
        [Fact]
        public void Then_The_Model_Is_Built_Successfully()
        {
            //Arrange

            var employerAccountId = 1;
            var payeSchemes = new List<string> { "payeScheme" };

            //Act

            var result = new EmployerPayeSchemes(employerAccountId, payeSchemes);

            //Assert

            Assert.Equal(employerAccountId, result.EmployerAccountId);
            Assert.Equal(payeSchemes, result.PayeSchemes);
        }
    }
}