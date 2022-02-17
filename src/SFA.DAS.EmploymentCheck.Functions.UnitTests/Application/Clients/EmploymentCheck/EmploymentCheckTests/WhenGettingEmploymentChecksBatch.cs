using System;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheck.EmploymentCheckTests
{
    public class WhenGettingEmploymentChecksBatch
    {
        private readonly Mock<IEmploymentCheckService> _employmentCheckService;
        private readonly Mock<ILogger<IEmploymentCheckClient>> _logger;
        private readonly Fixture _fixture;

        public WhenGettingEmploymentChecksBatch()
        {
            _fixture = new Fixture();
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _logger = new Mock<ILogger<IEmploymentCheckClient>>();
        }

        [Test]
        public async Task Then_The_EmploymentCheckService_Is_Called()
        {
            //Arrange
            _employmentCheckService.Setup(x => x.GetEmploymentCheck())
                .ReturnsAsync(new List<Models.EmploymentCheck>());

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            await sut.GetEmploymentCheck();

            //Assert
            _employmentCheckService.Verify(x => x.GetEmploymentCheck(), Times.AtLeastOnce());
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_No_EmploymentChecks_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            _employmentCheckService.Setup(x => x.GetEmploymentCheck())
                .ReturnsAsync(new List<Models.EmploymentCheck>());

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            var result = await sut.GetEmploymentCheck();

            //Assert
            result.Should().BeEquivalentTo(new List<Models.EmploymentCheck>());
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            _employmentCheckService.Setup(x => x.GetEmploymentCheck())
                .ReturnsAsync((List<Models.EmploymentCheck>)null);

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            var result = await sut.GetEmploymentCheck();

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            //Arrange
            var employmentChecks = new List<Models.EmploymentCheck>
            {
                new Models.EmploymentCheck
                {
                    AccountId = 114,
                    ApprenticeshipId = 98,
                    CheckType = "CheckType55030a83-4cb3-4601-b092-667c72c0244a",
                    CorrelationId  = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45,53),
                    Employed = true,
                    Id = 41,
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    RequestCompletionStatus = 220,
                    Uln = 242
                }
            };

            _employmentCheckService.Setup(x => x.GetEmploymentCheck())
                .ReturnsAsync(employmentChecks);

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            var result = await sut.GetEmploymentCheck();

            //Assert
            Assert.AreEqual(employmentChecks, result);
        }
    }
}