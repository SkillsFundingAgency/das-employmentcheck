using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand;

namespace SFA.DAS.EmploymentCheck.Api.UnitTests.Mediators.Commands.RegisterCheckCommand
{
    public class WhenValidatingRegisterCheckCommand
    {
        private Api.Mediators.Commands.RegisterCheckCommand.RegisterCheckCommand _command;

        [SetUp]
        public void Setup()
        {
            _command = new Api.Mediators.Commands.RegisterCheckCommand.RegisterCheckCommand
            {
                ApprenticeshipAccountId = 1,
                ApprenticeshipId = 2,
                Uln = 3,
                CheckType = "CheckType",
                CorrelationId = Guid.NewGuid(),
                MinDate = DateTime.Today.AddDays(-1),
                MaxDate = DateTime.Today.AddDays(1)
            };
        }

        [Test]
        public void And_The_CorrelationId_Is_Missing_Then_An_Error_Is_Returned()
        {
            //Arrange
            _command.CorrelationId = Guid.Empty;

            var sut = new RegisterCheckCommandValidator();

            //Act

            var result = sut.Validate(_command);

            //Assert

            Assert.AreEqual("Missing data not supplied", result.ErrorMessage);
            Assert.AreEqual("Bad_Data", result.ErrorType);
        }
        [Test]
        public void And_The_CheckType_Is_Missing_Then_An_Error_Is_Returned()
        {
            //Arrange
            _command.CheckType = string.Empty;

            var sut = new RegisterCheckCommandValidator();

            //Act

            var result = sut.Validate(_command);

            //Assert

            Assert.AreEqual("Missing data not supplied", result.ErrorMessage);
            Assert.AreEqual("Bad_Data", result.ErrorType);
        }

        [Test]
        public void And_The_Uln_Is_Missing_Then_An_Error_Is_Returned()
        {
            //Arrange
            _command.Uln = 0;

            var sut = new RegisterCheckCommandValidator();

            //Act

            var result = sut.Validate(_command);

            //Assert

            Assert.AreEqual("Missing data not supplied", result.ErrorMessage);
            Assert.AreEqual("Bad_Data", result.ErrorType);
        }

        [Test]
        public void And_The_ApprenticeshipAccountId_Is_Missing_Then_An_Error_Is_Returned()
        {
            //Arrange
            _command.ApprenticeshipAccountId = 0;

            var sut = new RegisterCheckCommandValidator();

            //Act

            var result = sut.Validate(_command);

            //Assert

            Assert.AreEqual("Missing data not supplied", result.ErrorMessage);
            Assert.AreEqual("Bad_Data", result.ErrorType);
        }

        [Test]
        public void And_The_Dates_Are_Invalid_Then_An_Error_Is_Returned()
        {
            //Arrange
            _command.MaxDate = _command.MinDate;

            var sut = new RegisterCheckCommandValidator();

            //Act

            var result = sut.Validate(_command);

            //Assert

            Assert.AreEqual("Min date must be before Max date", result.ErrorMessage);
            Assert.AreEqual("Bad_DateRange", result.ErrorType);
        }

        [Test]
        public void And_The_Dates_Are_Invalid_And_There_Is_Missing_Data_Then_Multiple_Errors_Are_Returned()
        {
            //Arrange
            _command.CorrelationId = Guid.Empty;
            _command.MaxDate = _command.MinDate;

            var sut = new RegisterCheckCommandValidator();

            //Act

            var result = sut.Validate(_command);

            //Assert

            Assert.AreEqual("Missing data not supplied, Min date must be before Max date", result.ErrorMessage);
            Assert.AreEqual("Bad_Data, Bad_DateRange", result.ErrorType);
        }

        [Test]
        public void And_There_Are_No_Validation_Errors_Then_A_Blank_RegisterCheckResult_Is_Returned()
        {
            //Arrange

            var sut = new RegisterCheckCommandValidator();

            //Act

            var result = sut.Validate(_command);

            //Assert

            result.Should().BeEquivalentTo(new RegisterCheckResult());
        }
    }
}