using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Repositories;

namespace SFA.DAS.EmploymentCheck.Api.Tests.Application.Services.EmploymentCheckService
{
    public class WhenGettingLastId
    {
        private Mock<IEmploymentCheckRepository> _employmentCheckRepository;

        [SetUp]
        public void Setup()
        {
            _employmentCheckRepository = new Mock<IEmploymentCheckRepository>();
        }

        [Test]
        public async Task Then_The_Repository_Is_Called()
        {
            //Arrange

            var sut = new Api.Application.Services.EmploymentCheckService(_employmentCheckRepository.Object);

            //Act

            await sut.GetLastId();

            //Assert

            _employmentCheckRepository.Verify(x => x.GetLastId(), Times.Once);
        }

        [Test]
        public async Task Then_The_LastId_Is_Returned()
        {
            //Arrange

            var lastId = 10;

            _employmentCheckRepository.Setup(x => x.GetLastId()).ReturnsAsync(lastId);

            var sut = new Api.Application.Services.EmploymentCheckService(_employmentCheckRepository.Object);

            //Act

            var result = await sut.GetLastId();

            //Assert

            Assert.AreEqual(result, lastId);
        }
    }
}