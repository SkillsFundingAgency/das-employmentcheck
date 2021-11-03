using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Clients.EmploymentCheck.EmploymentCheckClientTests
{
    public class WhenGettingApprentices
    {
        private readonly Mock<EmploymentCheckDbConfiguration> _employmentCheckDbConfiguration;
        private readonly Mock<IEmploymentCheckService> _employmentCheckService;
        private readonly Mock<ILoggerAdapter<IEmploymentCheckClient>> _logger;

        public WhenGettingApprentices()
        {
            _employmentCheckDbConfiguration = new Mock<EmploymentCheckDbConfiguration>();
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _logger = new Mock<ILoggerAdapter<IEmploymentCheckClient>>();
        }

        [Fact]
        public async void Then_The_EmploymentCheckService_Is_Called()
        {
            //Arrange

            _employmentCheckService.Setup(x => x.GetApprentices()).ReturnsAsync(new List<Apprentice>());

            var sut = new EmploymentCheckClient(_employmentCheckDbConfiguration.Object, _employmentCheckService.Object,
                _logger.Object);

            //Act

            await sut.GetApprentices();

            //Assert

            _employmentCheckService.Verify(x => x.GetApprentices(), Times.Exactly(1));
        }

        [Fact]
        public async void And_No_Apprentices_Are_Returned_From_The_EmploymentCheckService_Then_That_Is_Logged_And_An_Empty_List_Returned()
        {
            //Arrange

            _employmentCheckService.Setup(x => x.GetApprentices()).ReturnsAsync(new List<Apprentice>());

            var sut = new EmploymentCheckClient(_employmentCheckDbConfiguration.Object, _employmentCheckService.Object,
                _logger.Object);

            //Act

            var result = await sut.GetApprentices();

            //Assert

            _logger.Verify(x => x.LogInformation("EmploymentCheckClient.GetApprentices(): returned null/zero apprentices"));
            result.Should().BeEquivalentTo(new List<Apprentice>());
        }

        [Fact]
        public async void And_Null_Is_Returned_From_The_EmploymentCheckService_Then_That_Is_Logged_And_An_Empty_List_Returned()
        {
            //Arrange

            _employmentCheckService.Setup(x => x.GetApprentices()).ReturnsAsync((List<Apprentice>)null);

            var sut = new EmploymentCheckClient(_employmentCheckDbConfiguration.Object, _employmentCheckService.Object,
                _logger.Object);

            //Act

            var result = await sut.GetApprentices();

            //Assert

            _logger.Verify(x => x.LogInformation("EmploymentCheckClient.GetApprentices(): returned null/zero apprentices"));
            result.Should().BeEquivalentTo(new List<Apprentice>());
        }

        [Fact]
        public async void And_Apprentices_Are_Returned_From_The_EmploymentCheckService_Then_It_Is_Logged_And_The_Apprentices_Are_Returned()
        {
            //Arrange

            var apprentice = new Apprentice(
                1,
                1,
                "1000001",
                1000001,
                1000001,
                1,
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));
            var apprentices = new List<Apprentice> {apprentice};

            _employmentCheckService.Setup(x => x.GetApprentices()).ReturnsAsync(apprentices);

            var sut = new EmploymentCheckClient(_employmentCheckDbConfiguration.Object, _employmentCheckService.Object,
                _logger.Object);

            //Act

            var result = await sut.GetApprentices();

            //Assert

            _logger.Verify(x => x.LogInformation($"EmploymentCheckClient.GetApprentices(): returned [{apprentices.Count}] apprentices"));
            Assert.Equal(apprentices, result);
        }

        [Fact]
        public async void
            And_The_EmploymentCheckClient_Throws_An_Exception_Then_It_Is_Logged_And_An_Empty_List_Is_Returned()
        {
            //Arrange

            var exception = new Exception("exception");

            _employmentCheckService.Setup(x => x.GetApprentices()).ThrowsAsync(exception);

            var sut = new EmploymentCheckClient(_employmentCheckDbConfiguration.Object, _employmentCheckService.Object,
                _logger.Object);

            //Act

            var result = await sut.GetApprentices();

            //Assert

            _logger.Verify(x => x.LogInformation($"\n\nEmploymentCheckClient.GetApprentices(): Exception caught - {exception.Message}. {exception.StackTrace}"));
            result.Should().BeEquivalentTo(new List<Apprentice>());
        }

        [Fact(Skip = "Connection string hard coded in class for stub usage")]
        public async void And_The_Connection_String_Is_Not_Configured_Then_It_Is_Logged_And_An_Empty_List_Is_Returned()
        {
            //Arrange

            _employmentCheckDbConfiguration.Object.ConnectionString = null;

            var sut = new EmploymentCheckClient(_employmentCheckDbConfiguration.Object, _employmentCheckService.Object,
                _logger.Object);

            //Act

            var result = await sut.GetApprentices();

            //Assert

            _logger.Verify(x => x.LogInformation($"\n\nEmploymentCheckClient.GetApprentices(): Employment Check Db Connection String NOT Configured"));
            result.Should().BeEquivalentTo(new List<Apprentice>());
        }
    }
}