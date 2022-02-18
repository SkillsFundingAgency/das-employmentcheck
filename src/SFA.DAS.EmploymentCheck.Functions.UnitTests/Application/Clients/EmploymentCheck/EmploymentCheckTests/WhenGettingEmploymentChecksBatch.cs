using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheck.EmploymentCheckTests
{
    public class WhenGettingEmploymentChecksBatch
    {
        private IEmploymentCheckService _sut;
        private Fixture _fixture;
        private Mock<IEmploymentCheckRepository> _employmentCheckRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckRepositoryMock = new Mock<IEmploymentCheckRepository>();

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
        public async Task Then_The_Repository_Is_Called()
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
        public async Task And_The_Repository_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            // Arrange
            var employmentChecks = _fixture.CreateMany<Models.EmploymentCheck>().ToList();

            _employmentCheckService.Setup(x => x.GetEmploymentCheck())
                .ReturnsAsync(employmentChecks);

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            var result = await sut.GetEmploymentCheck();

            // Assert
            Assert.AreEqual(employmentChecks, result);
        }
    }
}