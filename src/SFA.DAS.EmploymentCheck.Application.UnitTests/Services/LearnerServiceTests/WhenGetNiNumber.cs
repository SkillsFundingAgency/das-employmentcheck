using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.LearnerServiceTests
{
    public class WhenGetNiNumber
    {
        private ILearnerService _sut;
        private Fixture _fixture;
        private Mock<IDataCollectionsResponseRepository> _repositoryMock;
        private Mock<INationalInsuranceNumberService> _niServiceMock;
        private Data.Models.EmploymentCheck _employmentCheck;
        private Mock<ILogger<LearnerService>> _logger;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _logger = new Mock<ILogger<LearnerService>>();
            _repositoryMock = new Mock<IDataCollectionsResponseRepository>();
            _niServiceMock = new Mock<INationalInsuranceNumberService>();
            _employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Create();

            _sut = new LearnerService(
                _repositoryMock.Object,
                _niServiceMock.Object
            );
        }

        [Test]
        public async Task Then_NIService_Is_Called()
        {
            // Act
            await _sut.GetNiNumber(_employmentCheck);

            // Assert
            _niServiceMock.Verify(_ => _.Get(It.Is<NationalInsuranceNumberRequest>(r =>
                 r.EmploymentCheck == _employmentCheck &&
                 r.AcademicYear == null)));
        }


        [Test]
        public async Task Then_ReturnedNiNo_Is_Returned_To_Caller()
        {
            // Arrange
            var nino = _fixture.Build<LearnerNiNumber>()
                .With(n => n.Uln, _employmentCheck.Uln)
                .Without(n => n.NiNumber)
                .With(n => n.HttpStatusCode, HttpStatusCode.OK)
                .Create();
            nino.NiNumber = _fixture.Create<string>()[..20];

            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent($"[{{\"uln\":{nino.Uln},\"niNumber\":\"{nino.NiNumber}\"}}]"),
                StatusCode = HttpStatusCode.OK
            };

            _niServiceMock.Setup(_ => _.Get(It.Is<NationalInsuranceNumberRequest>(r => r.EmploymentCheck == _employmentCheck)))
                .ReturnsAsync(nino);

            // Act
            var result = await _sut.GetNiNumber(_employmentCheck);

            // Assert
            result.Should().BeEquivalentTo(nino);
        }
    }
}