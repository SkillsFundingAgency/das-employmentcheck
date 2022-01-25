using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Clients.SubmitLearnerData.SubmitLearnerDataClientTests
{
    public class WhenGettingLearnersNiNumber
    {
        private readonly Mock<ILearnerService> _submitLearnerDataService;
        private readonly Mock<ILogger<IEmploymentCheckClient>> _logger;
        private readonly Fixture _fixture;

        public WhenGettingLearnersNiNumber()
        {
            _fixture = new Fixture();
            _submitLearnerDataService = new Mock<ILearnerService>();
            _logger = new Mock<ILogger<IEmploymentCheckClient>>();
        }

        [Test]
        public async Task Then_The_SubmitLeanerDataService_Is_Called()
        {
            //Arrange
            var apprentices = new List<EmploymentCheck.Data.Models.EmploymentCheck> { _fixture.Create<EmploymentCheck.Data.Models.EmploymentCheck>() };

            _submitLearnerDataService.Setup(x => x.GetNiNumbers(apprentices))
                .ReturnsAsync(new List<LearnerNiNumber>());

            var sut = new LearnerClient(_logger.Object, _submitLearnerDataService.Object);

            //Act
            await sut.GetNiNumbers(apprentices);

            //Assert
            _submitLearnerDataService.Verify(x => x.GetNiNumbers(apprentices), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_LearnerService_Returns_No_Ni_Numbers_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var apprentices = new List<EmploymentCheck.Data.Models.EmploymentCheck> { _fixture.Create<EmploymentCheck.Data.Models.EmploymentCheck>() };

            _submitLearnerDataService.Setup(x => x.GetNiNumbers(apprentices))
                .ReturnsAsync(new List<LearnerNiNumber>());

            var sut = new LearnerClient(_logger.Object, _submitLearnerDataService.Object);

            //Act
            var result = await sut.GetNiNumbers(apprentices);

            //Assert
            result.Should().BeEquivalentTo(new List<LearnerNiNumber>());
        }
        [Test]
        public async Task And_The_LearnerService_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var apprentices = new List<EmploymentCheck.Data.Models.EmploymentCheck> { _fixture.Create<EmploymentCheck.Data.Models.EmploymentCheck>() };

            _submitLearnerDataService.Setup(x => x.GetNiNumbers(apprentices))
                .ReturnsAsync((List<LearnerNiNumber>)null);

            var sut = new LearnerClient(_logger.Object, _submitLearnerDataService.Object);

            //Act
            var result = await sut.GetNiNumbers(apprentices);

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task And_The_LearnerService_Returns_Ni_Numbers_Then_They_Are_Returned()
        {
            //Arrange
            var apprentices = new List<EmploymentCheck.Data.Models.EmploymentCheck> { _fixture.Create<EmploymentCheck.Data.Models.EmploymentCheck>() };
            var niNumber = new LearnerNiNumber(1000001, "1000001");
            var niNumbers = new List<LearnerNiNumber> { niNumber };

            _submitLearnerDataService.Setup(x => x.GetNiNumbers(apprentices))
                .ReturnsAsync(niNumbers);

            var sut = new LearnerClient(_logger.Object, _submitLearnerDataService.Object);

            //Act
            var result = await sut.GetNiNumbers(apprentices);

            //Assert
            Assert.AreEqual(niNumbers, result);
        }
    }
}