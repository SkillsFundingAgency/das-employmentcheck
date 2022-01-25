using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheckServiceTests
{
    public class WhenInsertingAnEmploymentCheck
    {
        private Mock<IEmploymentCheckRepository> _employmentCheckRepository;
        private Mock<Data.Models.EmploymentCheck> _employmentCheck;
        private EmploymentCheckService _sut;

        [SetUp]
        public void Setup()
        {
            //Arrange

            _employmentCheckRepository = new Mock<IEmploymentCheckRepository>();
            _employmentCheck = new Mock<Data.Models.EmploymentCheck>();

            _sut = new EmploymentCheckService(
                null,
                Mock.Of<ApplicationSettings>(),
                null,
                _employmentCheckRepository.Object,
                null);
        }

        [Test]
        public void Then_The_Repository_Is_Called()
        {
            _sut.InsertEmploymentCheck(_employmentCheck.Object);

            //Assert

            _employmentCheckRepository.Verify(x => x.Insert(_employmentCheck.Object), Times.Once);
        }
    }
}