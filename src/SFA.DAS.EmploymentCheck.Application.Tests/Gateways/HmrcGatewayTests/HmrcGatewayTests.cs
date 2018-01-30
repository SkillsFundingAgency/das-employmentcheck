using System;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Gateways;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.EmploymentCheck.Application.Tests.Gateways.HmrcGatewayTests
{
    [TestFixture]
    public class HmrcGatewayTests
    {
        private Mock<ITokenServiceApiClient> _tokenService;
        private Mock<IApprenticeshipLevyApiClient> _apprenticeshipLevyService;
        private HmrcGateway _target;

        [SetUp]
        public void SetUp()
        {
            _tokenService = new Mock<ITokenServiceApiClient>();
            _apprenticeshipLevyService = new Mock<IApprenticeshipLevyApiClient>();
            _target = new HmrcGateway(_tokenService.Object, _apprenticeshipLevyService.Object);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task WhenAnEmploymentCheckIsRequestedThenTheEmployedValueFromHmrcIsReturned(bool expectedResult)
        {
            var expectedToken = "ABC12345";

            _tokenService.Setup(x => x.GetPrivilegedAccessTokenAsync()).ReturnsAsync(new PrivilegedAccessToken { AccessCode = expectedToken });
            var levyServiceResponse = new EmploymentStatus {  Employed = expectedResult };

            var payeScheme = "ABC/123";
            var nationalInsuranceNumber = "AB123456C";
            var startDate = DateTime.Now.AddYears(-1);
            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(expectedToken, payeScheme, nationalInsuranceNumber, startDate, DateTime.Now.Date)).ReturnsAsync(levyServiceResponse);

            var result = await _target.IsNationalInsuranceNumberRelatedToPayeScheme(payeScheme, nationalInsuranceNumber, startDate);

            Assert.AreEqual(expectedResult, result);
        }
    }
}
