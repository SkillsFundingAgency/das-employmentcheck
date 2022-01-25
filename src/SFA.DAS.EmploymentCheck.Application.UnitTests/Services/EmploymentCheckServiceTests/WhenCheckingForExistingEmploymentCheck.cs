using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheckServiceTests
{
    public class WhenCheckingForExistingEmploymentCheck
    {
        private Mock<IEmploymentCheckRepository> _employmentCheckRepository;
        private Guid _correlationId;
        private EmploymentCheckService _sut;

        [SetUp]
        public void Setup()
        {
            _employmentCheckRepository = new Mock<IEmploymentCheckRepository>();
            _correlationId = Guid.NewGuid();

            _sut = new EmploymentCheckService(
                null,
                Mock.Of<ApplicationSettings>(),
                null,
                _employmentCheckRepository.Object,
                null);
        }

        [Test]
        public async Task Then_The_Repository_Is_Called()
        {

            //Act

            await _sut.GetLastEmploymentCheck(_correlationId);

            //Assert

            _employmentCheckRepository.Verify(x => x.GetEmploymentCheck(_correlationId), Times.Once);
        }

        [Test]
        public async Task And_No_EmploymentCheck_Exists_Then_Null_Is_Returned()
        {
            //Arrange

            _employmentCheckRepository.Setup(x => x.GetEmploymentCheck(_correlationId))
                .ReturnsAsync((Data.Models.EmploymentCheck) null);

            //Act

            var result = await _sut.GetLastEmploymentCheck(_correlationId);

            //Assert

            Assert.IsNull(result);
        }

        [Test]
        public async Task And_An_EmploymentCheck_Exists_Then_It_Is_Returned()
        {
            //Arrange

            var employmentCheck = new Mock<Data.Models.EmploymentCheck>();

            _employmentCheckRepository.Setup(x => x.GetEmploymentCheck(_correlationId))
                .ReturnsAsync(employmentCheck.Object);

            //Act

            var result = await _sut.GetLastEmploymentCheck(_correlationId);

            //Assert

            Assert.AreEqual(result, employmentCheck.Object);
        }
    }
}