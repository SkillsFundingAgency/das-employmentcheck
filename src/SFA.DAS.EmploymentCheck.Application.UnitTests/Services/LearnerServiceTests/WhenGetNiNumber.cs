using System.Net;
using System.Net.Http;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.LearnerServiceTests
{
    public class WhenGetNiNumber
    {
        private ILearnerService _sut;
        private Fixture _fixture;
        private Mock<IDataCollectionsResponseRepository> _repositoryMock;
        private Mock<IDataCollectionsApiClient<DataCollectionsApiConfiguration>> _apiClientMock;
        private Data.Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _apiClientMock = new Mock<IDataCollectionsApiClient<DataCollectionsApiConfiguration>>();
            _repositoryMock = new Mock<IDataCollectionsResponseRepository>();
            _employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Create();

            _sut = new LearnerService(
                Mock.Of<ILogger<ILearnerService>>(),
                _apiClientMock.Object,
                _repositoryMock.Object);
        }

        [Test]
        public async Task Then_DcApi_Is_Called()
        {
            // Act
            await _sut.GetNiNumber(_employmentCheck);

            // Assert
            _apiClientMock.Verify(_ => _.Get(It.Is<GetNationalInsuranceNumberRequest>(r => 
                r.GetUrl == $"/api/v1/ilr-data/learnersNi/2122?ulns={_employmentCheck.Uln}")));
        }

        [Test]
        public async Task Then_ReturnedNiNo_Is_Saved()
        {
            // Arrange
            var nino = _fixture.Build<LearnerNiNumber>()
                .With(n => n.Uln, _employmentCheck.Uln)
                .Without(n => n.NiNumber)
                .Create();
            nino.NiNumber = _fixture.Create<string>()[..20];

        var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent($"[{{\"uln\":{nino.Uln},\"niNumber\":\"{nino.NiNumber}\"}}]"),
                StatusCode = HttpStatusCode.OK
            };

            _apiClientMock.Setup(_ => _.Get(It.Is<GetNationalInsuranceNumberRequest>(
                    r => r.GetUrl == $"/api/v1/ilr-data/learnersNi/2122?ulns={_employmentCheck.Uln}")))
                .ReturnsAsync(httpResponse);

            // Act
            await _sut.GetNiNumber(_employmentCheck);

            // Assert
            _repositoryMock.Verify(_ => _.InsertOrUpdate(It.Is<DataCollectionsResponse>(
                    r => r.NiNumber == nino.NiNumber))
                , Times.Once());
        }
    }
}