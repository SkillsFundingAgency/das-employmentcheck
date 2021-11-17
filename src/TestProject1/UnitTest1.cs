using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Commands.CheckApprentice;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Services;
using SFA.DAS.TokenService.Api.Client;

namespace TestProject1
{
    public class Tests
    {
        private IHmrcService hmrcService;
        private ITokenServiceApiClient tokenzervice;
        private IApprenticeshipLevyApiClient apprenticeshipLevyService;
        private HmrcService2 _sut;

        [SetUp]
        public void Setup()
        {
            //ITokenServiceApiClientConfiguration configuration= new TokenServiceApiClientConfiguration();
            //tokenzervice = new TokenServiceApiClient(configuration);
            //builder.Services.Configure<AccountsApiConfiguration>(config.GetSection("AccountsInnerApi"));
            //builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<AccountsApiConfiguration>>().Value);
            //builder.Services.Configure<HmrcApiSettings>(config.GetSection("HmrcApiSettings"));
            //builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<HmrcApiSettings>>().Value);
            //builder.Services.Configure<TokenServiceApiClientConfiguration>(config.GetSection("TokenService"));
            //builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<TokenServiceApiClientConfiguration>>().Value);
            //builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));
            //builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<ApplicationSettings>>().Value);

          //  hmrcService = new HmrcService(tokenzervice, apprenticeshipLevyService, null);
            _sut = new HmrcService2();

        }

        [Test]
        public async Task Test1()
        {
            var result = await _sut.IsNationalInsuranceNumberRelatedToPayeScheme();
            Assert.IsTrue(result);
        }

        [Test]
        //[TestCase(1)]
        //[TestCase(2)]
        //[TestCase(3)]
        //[TestCase(4)]
        //[TestCase(10)]
        //[TestCase(11)]
        //[TestCase(13)]
        //[TestCase(14)]
        //[TestCase(15)]
        [TestCase(1000)]
        public async Task PerformanceTest(int noOfCalls)
        {
            await _sut.BombIt(noOfCalls);
            Assert.Pass();
        }
    }
}