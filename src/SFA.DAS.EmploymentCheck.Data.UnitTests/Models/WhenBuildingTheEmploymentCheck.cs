using AutoFixture;
using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.EmploymentCheck.Data.UnitTests.Models
{
    public class WhenBuildingTheEmploymentCheck
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void With_Employed_Null_Then_ErrorType_Has_Assigned_Value()
        {
            // Act
            var result = _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.Employed).With(x => x.ErrorType, "HmrcFailure").Create();

            // Assert
            result.Employed.Should().BeNull();
            result.ErrorType.Should().Be("HmrcFailure");
        }

        [Test]
        public void With_Employed_False_Then_ErrorType_Has_Assigned_Value()
        {
            // Act
            var result = _fixture.Build<Data.Models.EmploymentCheck>().With(x => x.Employed, false).Create();

            // Assert
            result.Employed.Should().BeFalse();
            result.ErrorType.Should().NotBe("HmrcFailure");
        }

        [Test]
        public void With_Employed_True_Then_ErrorType_Has_Assigned_Value()
        {
            // Act
            var result = _fixture.Build<Data.Models.EmploymentCheck>().With(x => x.Employed, true).Create();

            // Assert
            result.Employed.Should().BeTrue();
            result.ErrorType.Should().NotBe("HmrcFailure");
        }
    }
}