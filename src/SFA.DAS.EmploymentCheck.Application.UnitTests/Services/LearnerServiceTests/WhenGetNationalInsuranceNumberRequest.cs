using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.LearnerServiceTests
{
    public class WhenGetNationalInsuranceNumberRequest
    {
        [Test]
        public void Then_GetNationalInsuranceNumberRequest_With_Url_Containing_Uln()
        {
            // Arrange
            long uln = 9999L;
            var expectedUrl = $"/api/v1/ilr-data/learnersNi/2122?ulns={uln}";
            GetNationalInsuranceNumberRequest sut = new GetNationalInsuranceNumberRequest(uln);

            // Act
            var url = sut.GetUrl;

            // Assert
            url.Should().Be(expectedUrl);

        }
    }
}
