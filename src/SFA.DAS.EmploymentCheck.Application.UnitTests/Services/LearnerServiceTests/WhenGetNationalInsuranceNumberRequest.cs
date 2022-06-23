using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.LearnerServiceTests
{
    public class WhenGetNationalInsuranceNumberRequest
    {
        private GetNationalInsuranceNumberRequest _sut;
        private Fixture _fixture;
        private DataCollectionsApiConfiguration _apiApiConfiguration;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _apiApiConfiguration = new DataCollectionsApiConfiguration();
        }

        [Test]
        public void WhenGetNationalInsuranceNumberRequest_Should_Return_Url()
        {
            // Arrange and Act
            var uln = _fixture.Create<long>();
            _apiApiConfiguration.Path = "a" + _fixture.Create<string>();
            _sut = new GetNationalInsuranceNumberRequest(uln, _apiApiConfiguration);

            // Assert
            _sut.GetUrl.Should().Be($"/{_apiApiConfiguration.Path}?ulns={uln}");
        }

        [Test]
        public void WhenGetNationalInsuranceNumberRequest_Should_Return_Url_when_prefixed_with_forward_slash()
        {
            // Arrange and Act
            var uln = _fixture.Create<long>();
            _apiApiConfiguration.Path = "/" + _fixture.Create<string>();
            _sut = new GetNationalInsuranceNumberRequest(uln, _apiApiConfiguration);

            // Assert
            _sut.GetUrl.Should().Be($"{_apiApiConfiguration.Path}?ulns={uln}");
        }
    }
}
