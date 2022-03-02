using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Helpers;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.EmploymentCheck.Data.UnitTests.Helpers
{
    public class WhenCreateEmploymentCheckCacheRequest
    {
        private const string NINO = "AB123456C_1234567890";
        private const string PAYE = "Paye001";

        private Fixture _fixture;
        private IEmploymentCheckCacheRequestFactory _sut;
        private Data.Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
        }

        [Test]
        public async Task Then_A_CreateEmploymentCheckCacheRequest_Is_Created()
        {
            // Arrange
            _sut = new EmploymentCheckCacheRequestFactory();

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequest(_employmentCheck, NINO, PAYE);

            // Assert
            result.CorrelationId.Should().Be(_employmentCheck.CorrelationId);
            result.Nino.Should().Be(NINO);
            result.PayeScheme.Should().Be(PAYE);
            result.MinDate.Should().Be(_employmentCheck.MinDate);
            result.MaxDate.Should().Be(_employmentCheck.MaxDate);
            result.Employed.Should().Be(_employmentCheck.Employed);
            result.RequestCompletionStatus.Should().Be(_employmentCheck.RequestCompletionStatus);
        }

        [Test]
        public async Task Then_When_The_Nino_Length_Too_Long_It_Is_Truncated_And_The_EmploymentCheckCacheRequest_Is_Created()
        {
            // Arrange
            _sut = new EmploymentCheckCacheRequestFactory();

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequest(_employmentCheck, NINO + "123456789012345678901234567890", PAYE);

            // Assert
            result
                .Should()
                .BeEquivalentTo(result, opts => opts
                .Excluding(x => x.Nino));

            result.Nino.Should().Be(NINO);
        }
    }
}