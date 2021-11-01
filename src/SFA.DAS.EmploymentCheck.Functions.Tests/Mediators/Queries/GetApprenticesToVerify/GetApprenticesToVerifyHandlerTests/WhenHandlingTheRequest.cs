using System;
using System.Collections.Generic;
using System.Threading;
using Dynamitey.DynamicObjects;
using FluentAssertions;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesToVerify;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprenticesToVerify.GetApprenticesToVerifyHandlerTests
{
    public class WhenHandlingTheRequest
    {
        private readonly Mock<IEmploymentCheckService> _employmentCheckService;
        private readonly Mock<ILoggerAdapter<GetApprenticesToVerifyHandler>> _logger;

        public WhenHandlingTheRequest()
        {
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _logger = new Mock<ILoggerAdapter<GetApprenticesToVerifyHandler>>();
        }

        [Fact]
        public async void Then_The_EmploymentCheckService_Is_Called()
        {
            //Arrange

            _employmentCheckService.Setup(x => x.GetApprentices()).ReturnsAsync(new List<Apprentice>());

            var sut = new GetApprenticesToVerifyHandler(_employmentCheckService.Object, _logger.Object);

            //Act

            await sut.Handle(new GetApprenticesToVerifyRequest(), CancellationToken.None);

            //Assert

            _employmentCheckService.Verify(x => x.GetApprentices(), Times.Exactly(1));
        }

        [Fact]
        public async void And_No_Apprentices_Returned_From_The_EmploymentcheckClient_Then_Result_Is_Logged_And_An_Empty_List_Returned()
        {
            //Arrange

            _employmentCheckService.Setup(x => x.GetApprentices()).ReturnsAsync(new List<Apprentice>());

            var sut = new GetApprenticesToVerifyHandler(_employmentCheckService.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetApprenticesToVerifyRequest(), CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation("GetApprenticesToVerifyHandler.Handle() returned null/zero learners"));
            result.ApprenticesToVerify.Should().BeEquivalentTo(new List<Apprentice>());
        }

        [Fact]
        public async void And_Null_Returned_From_The_EmploymentcheckClient_Then_Result_Is_Logged_And_An_Empty_List_Returned()
        {
            //Arrange

            _employmentCheckService.Setup(x => x.GetApprentices()).ReturnsAsync((List<Apprentice>)null);

            var sut = new GetApprenticesToVerifyHandler(_employmentCheckService.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetApprenticesToVerifyRequest(), CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation("GetApprenticesToVerifyHandler.Handle() returned null/zero learners"));
            result.ApprenticesToVerify.Should().BeEquivalentTo(new List<Apprentice>());
        }

        [Fact]
        public async void And_Apprentices_Returned_From_The_EmploymentcheckClient_Then_Result_Is_Logged()
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

            var sut = new GetApprenticesToVerifyHandler(_employmentCheckService.Object, _logger.Object);

            //Act

            await sut.Handle(new GetApprenticesToVerifyRequest(), CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation($"GetApprenticesToVerifyHandler.Handle() returned {apprentices.Count} learner(s)"));
        }

        [Fact]
        public async void And_Apprentices_Returned_From_The_EmploymentcheckClient_Then_Result_Is_Returned()
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
            var apprentices = new List<Apprentice> { apprentice };

            _employmentCheckService.Setup(x => x.GetApprentices()).ReturnsAsync(apprentices);

            var sut = new GetApprenticesToVerifyHandler(_employmentCheckService.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetApprenticesToVerifyRequest(), CancellationToken.None);

            //Assert
            
            Assert.Equal(apprentices, result.ApprenticesToVerify);
        }

        [Fact]
        public async void
            And_The_EmploymentCheckClient_Throws_An_Exception_Then_It_It_Logged_And_An_Empty_List_Is_Returned()
        {
            //Arrange

            var exception = new Exception("Exception");

            _employmentCheckService.Setup(x => x.GetApprentices()).ThrowsAsync(exception);

            var sut = new GetApprenticesToVerifyHandler(_employmentCheckService.Object, _logger.Object);

            //Act

            await sut.Handle(new GetApprenticesToVerifyRequest(), CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation($"GetApprenticesToVerifyHandler.Handle()\n\n Exception caught - {exception.Message}. {exception.StackTrace}"));
        }
    }
}