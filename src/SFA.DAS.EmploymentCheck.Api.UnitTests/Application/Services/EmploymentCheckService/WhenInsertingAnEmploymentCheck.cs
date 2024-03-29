﻿using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Repositories;

namespace SFA.DAS.EmploymentCheck.Api.UnitTests.Application.Services.EmploymentCheckService
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
        public async Task Then_The_Repository_Is_Called()
        {
            // Arrange
            var sut = new Api.Application.Services.EmploymentCheckService(_employmentCheckRepository.Object);

            // Act
            await sut.InsertEmploymentCheck(_employmentCheck.Object);

            // Assert
            _employmentCheckRepository.Verify(x => x.Insert(_employmentCheck.Object), Times.Once);
        }
    }
}