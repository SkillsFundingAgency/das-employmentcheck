using FluentAssertions;
using NUnit.Framework;
using System;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.LearnerServiceTests
{
    public class WhenGetAcademicYear
    {
        [Test]
        public void WhenGetAcademicYearJuly2022_Should_Return_2022()
        {
            // Arrange and Act
            int year = Application.Services.Learner.GetNationalInsuranceNumberRequest.GetAccademicYear(new DateTime(2022, 8, 31));

            // Assert
            year.Should().Be(2022);

        }

        [Test]
        public void WhenGetAcademicYearSeptember2022_Should_Return_2023()
        {
            // Arrange and Act
            int year = Application.Services.Learner.GetNationalInsuranceNumberRequest.GetAccademicYear(new DateTime(2022, 9, 1));

            // Assert
            year.Should().Be(2023);

        }

        [Test]
        public void WhenGetAcademicYear_Should_Return_CurrentDateYear()
        {
            // Arrange 
            Random rnd = new Random();
            int month = rnd.Next(1, 8);
            int day = rnd.Next(1, 28);

            // Act
            int year = Application.Services.Learner.GetNationalInsuranceNumberRequest.GetAccademicYear(new DateTime(DateTime.Now.Year, month, day));

            // Assert
            year.Should().Be(DateTime.Now.Year);

        }

        [Test]
        public void WhenGetAcademicYear_Should_Return_CurrentDateYearPlusOne()
        {
            // Arrange
            Random rnd = new Random();
            int month = rnd.Next(9, 12);
            int day = rnd.Next(1, 28);

            // Act
            int year = Application.Services.Learner.GetNationalInsuranceNumberRequest.GetAccademicYear(new DateTime(DateTime.Now.Year, month, day));

            // Assert
            year.Should().Be(DateTime.Now.Year + 1);

        }
    }
}
