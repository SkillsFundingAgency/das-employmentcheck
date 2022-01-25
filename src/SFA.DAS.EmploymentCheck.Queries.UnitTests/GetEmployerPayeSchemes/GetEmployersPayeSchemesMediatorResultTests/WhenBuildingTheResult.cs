using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.GetEmployerPayeSchemes.GetEmployersPayeSchemesMediatorResultTests
{
    public class WhenBuildingTheResult
    {
        [Test]
        public void Then_The_Result_Is_Built_Successfully()
        {
            //Arrange

            var payeScheme = new EmployerPayeSchemes(1000001, new List<string> {"payeScheme"});
            var payeSchemes = new List<EmployerPayeSchemes> {payeScheme};

            //Act

            var result = new GetPayeSchemesQueryResult(payeSchemes);

            //Assert

            Assert.AreEqual(payeSchemes, result.EmployersPayeSchemes);
        }
    }
}