using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmploymentCheckServiceTests
{
    public class WhenCreatingEmploymentCheckCacheRequests
    {
        private IEmploymentCheckService _sut;
        private Fixture _fixture;
        private Mock<IEmploymentCheckRepository> _employmentCheckRepositoryMock;
        private Mock<IEmploymentCheckCacheRequestRepository> _employmentCheckCashRequestRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckRepositoryMock = new Mock<IEmploymentCheckRepository>();
            _employmentCheckCashRequestRepositoryMock = new Mock<IEmploymentCheckCacheRequestRepository>();
            _sut = new EmploymentCheckService(
                Mock.Of<ILogger<IEmploymentCheckService>>(),
                _employmentCheckRepositoryMock.Object,
                _employmentCheckCashRequestRepositoryMock.Object,
                Mock.Of<IUnitOfWork>()
            );
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            // Arrange
            var nino = _fixture.Create<LearnerNiNumber>();
            var paye = _fixture.Build<EmployerPayeSchemes>().With(x => x.PayeSchemes, new[] { _fixture.Create<string>() }).Create();
            var check = _fixture.Build<Data.Models.EmploymentCheck>().With(c => c.Uln, nino.Uln).With(c => c.AccountId, paye.EmployerAccountId).Create();
            var employmentCheckData = new EmploymentCheckData(check, nino, paye);

            var expected = new List<EmploymentCheckCacheRequest>
            {
                new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = check.Id,
                    CorrelationId = check.CorrelationId,
                    Nino = nino.NiNumber,
                    PayeScheme = paye.PayeSchemes.FirstOrDefault(),
                    MaxDate = check.MaxDate,
                    MinDate = check.MinDate,
                    RequestCompletionStatus = null
                }
            };

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

            // Assert
            result.Should().BeEquivalentTo(expected);
            foreach (var employmentCheckCacheRequest in result)
            {
                _employmentCheckCashRequestRepositoryMock.Verify(r => r.Save(employmentCheckCacheRequest), Times.Once);
            }
        }
    }
}