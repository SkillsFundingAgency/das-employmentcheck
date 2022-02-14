using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheckClientTests
{
    public class WhenGettingEmploymentChecksBatch
    {

        private readonly Fixture _fixture;
        private readonly Mock<IEmploymentCheckService> _employmentCheckServiceMock = new Mock<IEmploymentCheckService>();
        private readonly EmploymentCheckClient _sut;

        public WhenGettingEmploymentChecksBatch()
        {
            _fixture = new Fixture();
            _sut = new EmploymentCheckClient(_employmentCheckServiceMock.Object);
        }

        [Test]
        public async Task Then_The_EmploymentCheckService_Is_Called()
        {
            // Arrange
            _employmentCheckServiceMock
                .Setup(x => x.GetEmploymentChecksBatch())
                .ReturnsAsync(new List<Models.EmploymentCheck>());

            // Act
            await _sut
                .GetEmploymentChecksBatch();

            // Assert
            _employmentCheckServiceMock
                .Verify(x => x.GetEmploymentChecksBatch(), Times.AtLeastOnce);
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            //Arrange
            var employmentChecks = _fixture
                .Build<Models.EmploymentCheck>()
                .CreateMany()
                .ToList();

            _employmentCheckServiceMock
                .Setup(x => x.GetEmploymentChecksBatch())
                .ReturnsAsync(employmentChecks);

            //Act
            var result = await _sut
                .GetEmploymentChecksBatch();

            //Assert
            Assert
                .AreEqual(employmentChecks, result);
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_No_EmploymentChecks_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            _employmentCheckServiceMock
                .Setup(x => x.GetEmploymentChecksBatch())
                .ReturnsAsync(new List<Models.EmploymentCheck>());

            //Act
            var result = await _sut
                .GetEmploymentChecksBatch();

            //Assert
            result
                .Should()
                .BeEquivalentTo(new List<Models.EmploymentCheck>());
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_Null_Then_An_Empty_List_Then_Null_Is_Returned()
        {
            //Arrange
            _employmentCheckServiceMock
                .Setup(x => x.GetEmploymentChecksBatch())
                .ReturnsAsync(() => null);

            //Act
            var result = await _sut
                .GetEmploymentChecksBatch();

            //Assert
            result
                .Should()
                .BeNull();
        }
    }
}