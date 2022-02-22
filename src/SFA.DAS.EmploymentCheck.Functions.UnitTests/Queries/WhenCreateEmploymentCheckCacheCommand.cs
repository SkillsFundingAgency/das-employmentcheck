﻿using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetHmrcLearnerEmploymentStatus;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Queries
{
    public class WhenGetHmrcLearnerEmploymentStatusQuery
    {
        private GetHmrcLearnerEmploymentStatusQueryHandler _sut;
        private Mock<IHmrcService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<IHmrcService>(); 
            _sut = new GetHmrcLearnerEmploymentStatusQueryHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_Service_is_called()
        {
            // Arrange
            var request = _fixture.Create<GetHmrcLearnerEmploymentStatusQueryRequest>();

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.IsNationalInsuranceNumberRelatedToPayeScheme(request.EmploymentCheckCacheRequest), Times.Once);
        }
    }
}