using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumbers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Queries.GetNiNumbers
{
    public class WhenHandlingTheRequest
    {
        private Mock<ILearnerClient> _submitLearnerDataClient;
        private Mock<ILogger<GetNiNumbersQueryHandler>> _logger;

        [SetUp]
        public void SetUp()
        {
            _submitLearnerDataClient = new Mock<ILearnerClient>();
            _logger = new Mock<ILogger<GetNiNumbersQueryHandler>>();
        }

        [Test]
        public async Task Then_The_SubmitLearnerDataClient_Is_Called()
        {
            //Arrange
            var request = new GetNiNumbersQueryRequest(new List<Functions.Application.Models.EmploymentCheck>());

            _submitLearnerDataClient.Setup(x => x.GetNiNumbers(request.EmploymentCheckBatch))
                .ReturnsAsync(new List<LearnerNiNumber>());

            var sut = new GetNiNumbersQueryHandler(_submitLearnerDataClient.Object, _logger.Object);

            //Act

            await sut.Handle(new GetNiNumbersQueryRequest(new List<Functions.Application.Models.EmploymentCheck>()), CancellationToken.None);

            //Assert

            _submitLearnerDataClient.Verify(x => x.GetNiNumbers(request.EmploymentCheckBatch), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_SubmitLearnerDataClient_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var request = new GetNiNumbersQueryRequest(new List<Functions.Application.Models.EmploymentCheck>());

            _submitLearnerDataClient.Setup(x => x.GetNiNumbers(request.EmploymentCheckBatch))
                .ReturnsAsync((List<LearnerNiNumber>)null);

            var sut = new GetNiNumbersQueryHandler(_submitLearnerDataClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetNiNumbersQueryRequest(new List<Functions.Application.Models.EmploymentCheck>()), CancellationToken.None);

            //Assert

            result.LearnerNiNumber.Should().BeEquivalentTo(new List<LearnerNiNumber>());
        }

        [Test]
        public async Task And_The_SubmitLearnerDataClient_Returns_LearnerNiNumbers_Then_They_Are_Returned()
        {
            //Arrange
            var request = new GetNiNumbersQueryRequest(new List<Functions.Application.Models.EmploymentCheck>());

            var niNumber = new LearnerNiNumber(1000001, "1000001");
            var niNumbers = new List<LearnerNiNumber> { niNumber };

            _submitLearnerDataClient.Setup(x => x.GetNiNumbers(request.EmploymentCheckBatch))
                .ReturnsAsync(niNumbers);

            var sut = new GetNiNumbersQueryHandler(_submitLearnerDataClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetNiNumbersQueryRequest(new List<Functions.Application.Models.EmploymentCheck>()), CancellationToken.None);

            //Assert

            result.LearnerNiNumber.Should().BeEquivalentTo(niNumbers);
        }
    }
}