using System;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Commands.CheckApprentice.CheckApprenticeCommandTests
{
    public class WhenBuildingCheckApprenticeCommand
    {
        [Fact]
        public void Then_The_Command_Is_Built_Correctly()
        {
            //Arrange
            var apprentice = new Apprentice(1,
                1,
                "1000001",
                1000001,
                1000001, 
                1000001, 
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(1));

            //Act
            var command = new CheckApprenticeCommand(apprentice);

            //Assert
            Assert.Equal(apprentice.Id, command.Apprentice.Id);
            Assert.Equal(apprentice.AccountId, command.Apprentice.AccountId);
            Assert.Equal(apprentice.NationalInsuranceNumber, command.Apprentice.NationalInsuranceNumber);
            Assert.Equal(apprentice.ULN, command.Apprentice.ULN);
            Assert.Equal(apprentice.UKPRN, command.Apprentice.UKPRN);
            Assert.Equal(apprentice.ApprenticeshipId, command.Apprentice.ApprenticeshipId);
            Assert.Equal(apprentice.StartDate, command.Apprentice.StartDate);
            Assert.Equal(apprentice.EndDate, command.Apprentice.EndDate);
        }
    }
}