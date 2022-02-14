using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner;

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

            _sut = new LearnerClient(Mock.Of<ILogger<ILearnerClient>>(), _submitLearnerDataService.Object);
        }

        [Test]
        public async Task Then_The_SubmitLeanerDataService_Is_Called()
        {
            //Arrange
            var apprentices = new List<Functions.Application.Models.EmploymentCheck> { _fixture.Create<Functions.Application.Models.EmploymentCheck>() };

            _submitLearnerDataService.Setup(x => x.GetNiNumbers(apprentices))
                .ReturnsAsync(new List<LearnerNiNumber>());

            //Act
            await _sut.GetNiNumbers(apprentices);

            //Assert
            _submitLearnerDataService.Verify(x => x.GetNiNumbers(apprentices), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_LearnerService_Returns_No_Ni_Numbers_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var apprentices = new List<Functions.Application.Models.EmploymentCheck> { _fixture.Create<Functions.Application.Models.EmploymentCheck>() };

            _submitLearnerDataService.Setup(x => x.GetNiNumbers(apprentices))
                .ReturnsAsync(new List<LearnerNiNumber>());

            //Act
            var result = await _sut.GetNiNumbers(apprentices);

            //Assert
            result.Should().BeEquivalentTo(new List<LearnerNiNumber>());
        }
        [Test]
        public async Task And_The_LearnerService_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var apprentices = new List<Functions.Application.Models.EmploymentCheck> { _fixture.Create<Functions.Application.Models.EmploymentCheck>() };

            _submitLearnerDataService.Setup(x => x.GetNiNumbers(apprentices))
                .ReturnsAsync((List<LearnerNiNumber>)null);

            //Act
            var result = await _sut.GetNiNumbers(apprentices);

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task And_The_LearnerService_Returns_Ni_Numbers_Then_They_Are_Returned()
        {
            //Arrange
            var apprentices = new List<Functions.Application.Models.EmploymentCheck> { _fixture.Create<Functions.Application.Models.EmploymentCheck>() };
            var niNumber = new LearnerNiNumber(1000001, "1000001");
            var niNumbers = new List<LearnerNiNumber> { niNumber };

            _submitLearnerDataService.Setup(x => x.GetNiNumbers(apprentices))
                .ReturnsAsync(niNumbers);

            //Act
            var result = await _sut.GetNiNumbers(apprentices);

            //Assert
            Assert.AreEqual(niNumbers, result);
        }
    }
}