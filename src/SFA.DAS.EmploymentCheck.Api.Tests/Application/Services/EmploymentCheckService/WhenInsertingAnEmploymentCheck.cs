using System;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Repositories;

namespace SFA.DAS.EmploymentCheck.Api.Tests.Application.Services.EmploymentCheckService
{
    public class WhenInsertingAnEmploymentCheck
    {
        private Mock<IEmploymentCheckRepository> _employmentCheckRepository;
        private Mock<Api.Application.Models.EmploymentCheck> _employmentCheck;

        [SetUp]
        public void Setup()
        {
            _employmentCheckRepository = new Mock<IEmploymentCheckRepository>();
            _employmentCheck = new Mock<Api.Application.Models.EmploymentCheck>();
        }

        [Test]
        public void Then_The_Repository_Is_Called()
        {
            //Arrange

            var sut = new Api.Application.Services.EmploymentCheckService(_employmentCheckRepository.Object);

            //Act

            sut.InsertEmploymentCheck(_employmentCheck.Object);

            //Assert

            _employmentCheckRepository.Verify(x => x.Insert(_employmentCheck.Object), Times.Once);
        }
    }
}