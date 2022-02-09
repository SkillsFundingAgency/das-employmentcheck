using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes;
using NUnit.Framework;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Queries.GetPayeSchemes
{
    public class WhenBuildingTheResult
    {
        [Test]
        public void Then_The_Result_Is_Built_Successfully()
        {
            //Arrange

            var payeScheme = new EmployerPayeSchemes(1000001, new List<string> { "payeScheme" });
            var payeSchemes = new List<EmployerPayeSchemes> { payeScheme };

            //Act

            var result = new GetPayeSchemesQueryResult(payeSchemes);

            //Assert

            Assert.AreEqual(payeSchemes, result.EmployersPayeSchemes);
        }
    }
}