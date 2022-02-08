using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheck.EmploymentCheckCacheRequestTests
{
    public class WhenStoringCacheRequestRelatedRequestsProcessingStatus
    {
        private readonly Mock<IEmploymentCheckService> _employmentCheckService;
        private readonly Mock<ILogger<IEmploymentCheckClient>> _logger;
        private readonly Fixture _fixture;
        private readonly EmploymentCheckCacheRequest _employmentCheckCacheRequest;
        private readonly IList<EmploymentCheckCacheRequest> _employmentCheckCacheRequests;
        private readonly Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> _status;

        public WhenStoringCacheRequestRelatedRequestsProcessingStatus()
        {
            _fixture = new Fixture();
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _logger = new Mock<ILogger<IEmploymentCheckClient>>();
            _employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
            _employmentCheckCacheRequests = new List<EmploymentCheckCacheRequest> { _fixture.Create<EmploymentCheckCacheRequest>()};
            _status = new Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>(_fixture.Create<EmploymentCheckCacheRequest>(), _fixture.Create<ProcessingCompletionStatus>());
        }

        [Test]
        public async Task Then_The_EmploymentCheckService_Is_Called()
        {
            //Arrange
            _employmentCheckService.Setup(x => x.StoreEmploymentCheckResult(_employmentCheckCacheRequest));

            var sut = new EmploymentCheckClient(_logger.Object, _employmentCheckService.Object);

            //Act
            await sut.StoreEmploymentCheckResult(_employmentCheckCacheRequest);

            //Assert
            _employmentCheckService.Verify(x => x.StoreEmploymentCheckResult(_employmentCheckCacheRequest), Times.AtLeastOnce());
        }
    }
}