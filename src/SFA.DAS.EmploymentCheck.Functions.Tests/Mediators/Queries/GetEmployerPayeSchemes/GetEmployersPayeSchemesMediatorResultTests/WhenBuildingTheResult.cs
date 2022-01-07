using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetEmployerPayeSchemes.GetEmployersPayeSchemesMediatorResultTests
{
    public class WhenBuildingTheResult
    {
        [Fact]
        public void Then_The_Result_Is_Built_Successfully()
        {
            //Arrange

            var payeScheme = new EmployerPayeSchemes(1000001, new List<string> {"payeScheme"});
            var payeSchemes = new List<EmployerPayeSchemes> {payeScheme};

            //Act

            var result = new GetPayeSchemesQueryResult(payeSchemes);

            //Assert

            Assert.Equal(payeSchemes, result.EmployersPayeSchemes);
        }
    }
}