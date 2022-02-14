using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheck.EmploymentCheckCacheRequestTests
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
                _employmentCheckCashRequestRepositoryMock.Object
            );
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            // Arrange
            IList<LearnerNiNumber> ninos = _fixture.CreateMany<LearnerNiNumber>().ToList();
            IList<EmployerPayeSchemes> paye = _fixture.Build<EmployerPayeSchemes>()
                .With(x => x.PayeSchemes, new[] { _fixture.Create<string>() })
                    .CreateMany().ToList();

            IList<Functions.Application.Models.EmploymentCheck> checks =
                new List<Functions.Application.Models.EmploymentCheck>
                {
                    _fixture.Build<Functions.Application.Models.EmploymentCheck>().With(c => c.Uln, ninos[0].Uln).With(c => c.AccountId, paye[0].EmployerAccountId).Create(),
                    _fixture.Build<Functions.Application.Models.EmploymentCheck>().With(c => c.Uln, ninos[1].Uln).With(c => c.AccountId, paye[1].EmployerAccountId).Create(),
                    _fixture.Build<Functions.Application.Models.EmploymentCheck>().With(c => c.Uln, ninos[2].Uln).With(c => c.AccountId, paye[2].EmployerAccountId).Create(),
                };

            var employmentCheckData = new EmploymentCheckData
            {
                ApprenticeNiNumbers = ninos,
                EmployerPayeSchemes = paye,
                EmploymentChecks = checks
            };

            var expected = checks.Select((t, i) => new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = t.Id,
                    CorrelationId = t.CorrelationId,
                    Nino = ninos[i].NiNumber,
                    PayeScheme = paye[i].PayeSchemes.FirstOrDefault(),
                    MaxDate = checks[i].MaxDate,
                    MinDate = checks[i].MinDate,
                    RequestCompletionStatus = null,
                })
                .ToList();


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