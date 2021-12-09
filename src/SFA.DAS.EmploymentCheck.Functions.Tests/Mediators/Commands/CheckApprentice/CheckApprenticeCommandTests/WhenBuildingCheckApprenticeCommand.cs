using AutoFixture;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Commands.CheckApprentice.CheckApprenticeCommandTests
{
    public class WhenBuildingCheckApprenticeCommand
    {
        [Fact]
        public void Then_The_Command_Is_Built_Correctly()
        {
            //Arrange
            var fixture = new Fixture();
            var apprentice = fixture.Create<ApprenticeEmploymentCheckMessageModel>();

            //Act
            var command = new CheckApprenticeEmploymentStatusQueryRequest(apprentice);

            //Assert
            Assert.Equal(apprentice, command.ApprenticeEmploymentCheckMessageModel);
        }
    }
}