using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Services.EmploymentCheckServiceTests
{
    public class WhenCreateEmploymentCheckCacheRequest
    {
        private Fixture _fixture;
        private IEmploymentCheckCacheRequestFactory _sut;
        private Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheck = _fixture.Create<Models.EmploymentCheck>();
        }

        [Test]
        public async Task Then_A_CreateEmploymentCheckCacheRequest_Is_Created()
        {
            // Arrange
            _sut = new EmploymentCheckCacheRequestFactory();

            // Act
            var result = await _sut
                .CreateEmploymentCheckCacheRequest(_employmentCheck, "AB123456C", "Paye1");

            // Assert
            result.Should()
                .Be(result);
        }

        [Test]
        public async Task Then_When_The_Nino_Length_Too_Long_It_Is_Truncated_And_The_EmploymentCheckCacheRequest_Is_Created()
        {
            // Arrange
            _sut = new EmploymentCheckCacheRequestFactory();

            // Act
            var result = await _sut
                .CreateEmploymentCheckCacheRequest(_employmentCheck, "AB123456C_123456789012345678901234567890", "Paye1");

            // Assert
            result
                .Should().BeEquivalentTo(result,
                opts => opts
                    .Excluding(x => x.Nino));

            result.Nino
                .Should()
                .Be("AB123456C_1234567890");
        }
    }
}