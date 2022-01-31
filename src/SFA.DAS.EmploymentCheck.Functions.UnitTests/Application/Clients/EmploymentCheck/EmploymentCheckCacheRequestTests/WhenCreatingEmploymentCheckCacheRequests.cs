using System;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheck.EmploymentCheckCacheRequestTests
{
    public class WhenCreatingEmploymentCheckCacheRequests
    {
        private readonly Mock<IEmploymentCheckService> _employmentCheckService;
        private readonly Mock<ILogger<IEmploymentCheckClient>> _logger;
        private readonly Fixture _fixture;

        public WhenCreatingEmploymentCheckCacheRequests()
        {
            _fixture = new Fixture();
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _logger = new Mock<ILogger<IEmploymentCheckClient>>();
        }

        [Test]
        public async Task Then_The_EmploymentCheckService_Is_Called()
        {
            //Arrange
            var employmentCheckData = _fixture.Create<EmploymentCheckData>();

            _employmentCheckService.Setup(x => x.CreateEmploymentCheckCacheRequests(employmentCheckData))
                .ReturnsAsync(new List<EmploymentCheckCacheRequest>());

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            await sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            //Assert
            _employmentCheckService.Verify(x => x.CreateEmploymentCheckCacheRequests(employmentCheckData), Times.AtLeastOnce());
        }

        [Test]
        public async Task And_The_EmploymentCheckService_CreateEmploymentCheckCacheRequests_Returns_No_Requests_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var employmentCheckData = _fixture.Create<EmploymentCheckData>();

            _employmentCheckService.Setup(x => x.CreateEmploymentCheckCacheRequests(employmentCheckData))
                .ReturnsAsync((List<EmploymentCheckCacheRequest>)null);

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            var result = await sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task And_The_EmploymentCheckService_CreateEmploymentCheckCacheRequests_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var employmentCheckData = _fixture.Create<EmploymentCheckData>();

            _employmentCheckService.Setup(x => x.CreateEmploymentCheckCacheRequests(employmentCheckData))
                .ReturnsAsync(new List<EmploymentCheckCacheRequest>());

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            var result = await sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            //Assert
            result.Should().BeEquivalentTo(new List<EmploymentCheckCacheRequest>());
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            //Arrange
            var employmentCheckData = _fixture.Create<EmploymentCheckData>();
            var employmentCheckCacheRequests = new List<EmploymentCheckCacheRequest>
            {
                new EmploymentCheckCacheRequest()
                {
                    Id = 1,
                    CorrelationId = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
                    Nino = "NI12345678",
                    PayeScheme = "Paye1",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    RequestCompletionStatus = 220,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                }
            };

            _employmentCheckService.Setup(x => x.CreateEmploymentCheckCacheRequests(employmentCheckData))
                .ReturnsAsync(employmentCheckCacheRequests);

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            var result = await sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            //Assert
            Assert.AreEqual(employmentCheckCacheRequests, result);
        }
    }
}