using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.NationalInsuranceNumberTests
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
            var academicYear = _fixture.Create<string>();
            _apiApiConfiguration.Path = "a" + _fixture.Create<string>();
            _sut = new GetNationalInsuranceNumberRequest(uln, academicYear, _apiApiConfiguration);

            // Assert
            _sut.GetUrl.Should().Be($"/{_apiApiConfiguration.Path}/{academicYear}?ulns={uln}");
        }

        [Test]
        public void WhenGetNationalInsuranceNumberRequest_Should_Return_Url_when_prefixed_with_forward_slash()
        {
            // Arrange and Act
            var uln = _fixture.Create<long>();
            var academicYear = _fixture.Create<string>();
            _apiApiConfiguration.Path = "/" + _fixture.Create<string>();
            _sut = new GetNationalInsuranceNumberRequest(uln, academicYear, _apiApiConfiguration);

            // Assert
            _sut.GetUrl.Should().Be($"{_apiApiConfiguration.Path}/{academicYear}?ulns={uln}");
        }

        [Test]
        public void WhenGetNationalInsuranceNumberRequest_Should_Return_Default_Url()
        {
            // Arrange and Act
            var uln = _fixture.Create<long>();
            var academicYear = _fixture.Create<string>();
            _sut = new GetNationalInsuranceNumberRequest(uln, academicYear, _apiApiConfiguration);

            // Assert
            _sut.GetUrl.Should().Be($"/api/v1/ilr-data/learnersNi/{academicYear}?ulns={uln}");
        }
    }
}
