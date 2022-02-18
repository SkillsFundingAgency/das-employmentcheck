using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.SubmitLearnerData.SubmitLearnerDataClientTests
{
    public class WhenGettingLearnersNiNumber
    {
        private Mock<ILearnerService> _submitLearnerDataService;
        private Fixture _fixture;
        private LearnerClient _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _submitLearnerDataService = new Mock<ILearnerService>();

            _sut = new LearnerClient(_submitLearnerDataService.Object);
        }

        [Test]
        public async Task Then_The_SubmitLeanerDataService_Is_Called()
        {
            //Arrange
            var check = _fixture.Create<Functions.Application.Models.EmploymentCheck>();

            _submitLearnerDataService.Setup(x => x.GetNiNumber(check))
                .ReturnsAsync(new LearnerNiNumber());

            //Act
            await sut.GetNiNumber(check);

            //Assert
            _submitLearnerDataService.Verify(x => x.GetNiNumber(check), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_LearnerService_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var check = _fixture.Create<Functions.Application.Models.EmploymentCheck>();

            _submitLearnerDataService.Setup(x => x.GetNiNumber(check))
                .ReturnsAsync((LearnerNiNumber)null);

            //Act
            var result = await sut.GetNiNumber(check);

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task And_The_LearnerService_Returns_Ni_Numbers_Then_They_Are_Returned()
        {
            //Arrange
            var check = _fixture.Create<Functions.Application.Models.EmploymentCheck>();
            var niNumber = new LearnerNiNumber(1000001, "1000001");

            _submitLearnerDataService.Setup(x => x.GetNiNumber(check))
                .ReturnsAsync(niNumber);

            //Act
            var result = await sut.GetNiNumber(check);

            //Assert
            Assert.AreEqual(niNumber, result);
        }
    }
}