using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Interfaces.LearnerData;
using SFA.DAS.EmploymentCheck.Application.Clients.LearnerData;
using SFA.DAS.EmploymentCheck.Application.Interfaces.PaymentsCompliance;
using SFA.DAS.EmploymentCheck.Domain.Entities;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Clients.SubmitLearnerData.SubmitLearnerDataClientTests
{
    public class WhenGettingLearnersNiNumber
    {
        private readonly Mock<ILearnerDataService> _learnerDataService;
        private readonly Mock<ILogger<ILearnerDataClient>> _logger;
        private readonly Fixture _fixture;

        public WhenGettingLearnersNiNumber()
        {
            _fixture = new Fixture();
            _learnerDataService = new Mock<ILearnerDataService>();
            _logger = new Mock<ILogger<ILearnerDataClient>>();
        }

        [Test]
        public async Task Then_The_LeanerDataService_Is_Called()
        {
            //Arrange
            var employmentChecks = new List<Domain.Entities.EmploymentCheck> { _fixture.Create<Domain.Entities.EmploymentCheck>() };

            _learnerDataService.Setup(x => x.GetNiNumbers(employmentChecks))
                .ReturnsAsync(new List<LearnerNiNumber>());

            var sut = new LearnerDataClient(_logger.Object, _learnerDataService.Object);

            //Act
            await sut.GetNiNumbers(employmentChecks);

            //Assert
            _learnerDataService.Verify(x => x.GetNiNumbers(employmentChecks), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_LearnerService_Returns_No_Ni_Numbers_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var apprentices = new List<Domain.Entities.EmploymentCheck> { _fixture.Create<Domain.Entities.EmploymentCheck>() };

            _learnerDataService.Setup(x => x.GetNiNumbers(apprentices))
                .ReturnsAsync(new List<LearnerNiNumber>());

            var sut = new LearnerDataClient(_logger.Object, _learnerDataService.Object);

            //Act
            var result = await sut.GetNiNumbers(apprentices);

            //Assert
            result.Should().BeEquivalentTo(new List<LearnerNiNumber>());
        }
        [Test]
        public async Task And_The_LearnerService_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var apprentices = new List<Domain.Entities.EmploymentCheck> { _fixture.Create<Domain.Entities.EmploymentCheck>() };

            _learnerDataService.Setup(x => x.GetNiNumbers(apprentices))
                .ReturnsAsync((List<LearnerNiNumber>)null);

            var sut = new LearnerDataClient(_logger.Object, _learnerDataService.Object);

            //Act
            var result = await sut.GetNiNumbers(apprentices);

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task And_The_LearnerService_Returns_Ni_Numbers_Then_They_Are_Returned()
        {
            //Arrange
            var apprentices = new List<Domain.Entities.EmploymentCheck> { _fixture.Create<Domain.Entities.EmploymentCheck>() };
            var niNumber = new LearnerNiNumber(1000001, "1000001");
            var niNumbers = new List<LearnerNiNumber> { niNumber };

            _learnerDataService.Setup(x => x.GetNiNumbers(apprentices))
                .ReturnsAsync(niNumbers);

            var sut = new LearnerDataClient(_logger.Object, _learnerDataService.Object);

            //Act
            var result = await sut.GetNiNumbers(apprentices);

            //Assert
            Assert.AreEqual(niNumbers, result);
        }
    }
}