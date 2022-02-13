using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Services.EmploymentCheckServiceTests
{
    public class WhenCreatingEmploymentCheckCacheRequests
    {
        private const string NINO = "AB123456";
        private const string PAYE = "Paye001";

        private Fixture _fixture;

        private EmploymentCheckCacheRequest _employmentCheckCacheRequest;
        private Models.EmploymentCheck _employmentCheck;

        private IList<Models.EmploymentCheck> _employmentChecks;
        private IList<LearnerNiNumber> _apprenticeNiNumbers;
        private IList<EmployerPayeSchemes> _payeSchemes;
        private IList<EmploymentCheckCacheRequest> _employmentCheckCacheRequests;

        private Mock<ILogger<IEmploymentCheckService>> _logger;
        private Mock<IEmploymentCheckCacheRequestFactory> _cacheRequestFactory;
        private Mock<IEmploymentCheckRepository> _employmentCheckRepository;
        private Mock<IEmploymentCheckCacheRequestRepository> _employmentCheckCashRequestRepository;
        private IEmploymentCheckService _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _logger = new Mock<ILogger<IEmploymentCheckService>>(MockBehavior.Strict);
            _cacheRequestFactory = new Mock<IEmploymentCheckCacheRequestFactory>(MockBehavior.Strict);
            _cacheRequestFactory.SetupAllProperties();
            _employmentCheckRepository = new Mock<IEmploymentCheckRepository>(MockBehavior.Strict);
            _employmentCheckCashRequestRepository = new Mock<IEmploymentCheckCacheRequestRepository>(MockBehavior.Strict);
        }

        [Test]
        public async Task Then_EmploymentCheckCacheRequests_Is_Called()
        {
            // Arrange
            #region
            //var employmentChecks = new List<Models.EmploymentCheck>
            //{
            //    new Models.EmploymentCheck
            //    {
            //        Id = 1,
            //        AccountId = 1,
            //        ApprenticeshipId = 1,CheckType = "90 day",
            //        CorrelationId = Guid.NewGuid(),
            //        Uln = 12345678,
            //        Employed = false,
            //        RequestCompletionStatus = 1
            //    }
            //};


            //var employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
            //var employmentCheckCacheRequests = _fixture.CreateMany<EmploymentCheckCacheRequest>();
            //var id = 1L;
            //var apprenticeEmploymentCheckId = 2L;
            //var guid = Guid.NewGuid();
            //var nino = "A123456";
            //var paye = "Paye";
            //var minDate = new DateTime(2022, 2, 10);
            //var maxDate = new DateTime(2022, 2, 11);
            //var employed = true;
            //var requestCompletionStatus = (short)1;

            //_cacheRequestFactory.Setup(crf => crf.CreateEmploymentCheckCacheRequest(
            //    id,
            //     apprenticeEmploymentCheckId,
            //     guid,
            //     nino,
            //     paye,
            //     minDate,
            //     maxDate,
            //     employed,
            //     requestCompletionStatus))
            //    .ReturnsAsync(employmentCheckCacheRequest);
            #endregion

            #region
            _employmentCheck = _fixture.Build<Models.EmploymentCheck>()
                .With(e => e.Uln, 1)
                .With(e => e.AccountId, 1)
                .Create();
            #endregion

            _employmentChecks = new List<Models.EmploymentCheck> { _employmentCheck };
            var employmentCheckData = new EmploymentCheckData(_employmentChecks, _apprenticeNiNumbers, _payeSchemes);

            _cacheRequestFactory
                .Setup(r => r.CreateEmploymentCheckCacheRequest(_employmentCheck, NINO, PAYE))
                .ReturnsAsync(_employmentCheckCacheRequest);

            _employmentCheckCashRequestRepository
                .Setup(r => r.Save(_employmentCheckCacheRequest))
                .Returns(Task.FromResult(_employmentCheckCacheRequests));


            _sut = new EmploymentCheckService(
                _logger.Object,
                _employmentCheckRepository.Object,
                _employmentCheckCashRequestRepository.Object);

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequests(employmentCheckData, _cacheRequestFactory.Object);

            // Assert
            _employmentCheckCashRequestRepository.Verify(r => r.Save(_employmentCheckCacheRequest), Times.Exactly(1));
        }

        [Test]
        public async Task Then_If_SetCacheRequestRelatedRequestsProcessingStatus_Returns_EmploymentCheckCacheRequests_They_Are_Returned()
        {
            // Arrange
            var employmentChecks = _fixture.Build<Models.EmploymentCheck>().With(ec => ec.Uln, 1).CreateMany().ToList();
            var apprenticeNiNumbers = _fixture.Build<LearnerNiNumber>().With(lnn => lnn.Uln, 1).CreateMany().ToList();
            var payeSchemes = _fixture.Build<EmployerPayeSchemes>().With(paye => paye.EmployerAccountId, 1).CreateMany().ToList();
            var employmentCheck = new EmploymentCheckData(employmentChecks, apprenticeNiNumbers, payeSchemes);
            var employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
            var employmentCheckCacheRequests = _fixture.CreateMany<EmploymentCheckCacheRequest>();

            _employmentCheckCashRequestRepository.Setup(r => r.Save(employmentCheckCacheRequest))
                .Returns(Task.FromResult(employmentCheckCacheRequests));

            // Act
            var result = await _sut.CreateEmploymentCheckCacheRequests(employmentCheck, _cacheRequestFactory.Object);

            // Assert
            result.Should().BeEquivalentTo(employmentCheckCacheRequests);
        }

        //[Test]
        //public async Task Then_If_SetCacheRequestRelatedRequestsProcessingStatus_Returns_Null_Then_An_Empty_List_Is_Returned()
        //{
        //    // Arrange
        //    var status = _fixture.Create<Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>>();

        //    _employmentCheckCashRequestRepository.Setup(r => r.SetRelatedRequestsCompletionStatus(status))
        //        .ReturnsAsync(new List<EmploymentCheckCacheRequest>());

        //    // Act
        //    var result = await _sut.SetCacheRequestRelatedRequestsProcessingStatus(status);

        //    // Assert
        //    result.Should().BeEquivalentTo(new List<EmploymentCheckCacheRequest>());
        //}
    }
}