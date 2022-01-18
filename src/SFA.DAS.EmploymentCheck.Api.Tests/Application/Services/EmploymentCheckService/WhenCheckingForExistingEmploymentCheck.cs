using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Repositories;

namespace SFA.DAS.EmploymentCheck.Api.Tests.Application.Services.EmploymentCheckService
{
    public class WhenCheckingForExistingEmploymentCheck
    {
        private Mock<IEmploymentCheckRepository> _employmentCheckRepository;
        private Guid _correlationId;

        [SetUp]
        public void Setup()
        {
            _employmentCheckRepository = new Mock<IEmploymentCheckRepository>();
            _correlationId = Guid.NewGuid();
        }

        [Test]
        public async Task Then_The_Repository_Is_Called()
        {
            //Arrange

            var sut = new Api.Application.Services.EmploymentCheckService(_employmentCheckRepository.Object);

            //Act

            await sut.GetLastEmploymentCheck(_correlationId);

            //Assert

            _employmentCheckRepository.Verify(x => x.GetEmploymentCheck(_correlationId), Times.Once);
        }

        [Test]
        public async Task And_No_EmploymentCheck_Exists_Then_Null_Is_Returned()
        {
            //Arrange

            _employmentCheckRepository.Setup(x => x.GetEmploymentCheck(_correlationId))
                .ReturnsAsync((Api.Application.Models.EmploymentCheck) null);

            var sut = new Api.Application.Services.EmploymentCheckService(_employmentCheckRepository.Object);

            //Act

            var result = await sut.GetLastEmploymentCheck(_correlationId);

            //Assert

            Assert.IsNull(result);
        }

        [Test]
        public async Task And_An_EmploymentCheck_Exists_Then_It_Is_Returned()
        {
            //Arrange

            var employmentCheck = new Mock<Api.Application.Models.EmploymentCheck>();

            _employmentCheckRepository.Setup(x => x.GetEmploymentCheck(_correlationId))
                .ReturnsAsync(employmentCheck.Object);

            var sut = new Api.Application.Services.EmploymentCheckService(_employmentCheckRepository.Object);

            //Act

            var result = await sut.GetLastEmploymentCheck(_correlationId);

            //Assert

            Assert.AreEqual(result, employmentCheck.Object);
        }
    }
}