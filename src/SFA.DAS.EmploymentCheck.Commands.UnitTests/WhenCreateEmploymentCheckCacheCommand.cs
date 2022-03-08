using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Domain.Enums;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests
{
    public class WhenCreateEmploymentCheckCacheCommand
    {
        private CreateEmploymentCheckCacheRequestCommandHandler _sut;
        private Mock<IEmploymentCheckService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<IEmploymentCheckService>();
            _sut = new CreateEmploymentCheckCacheRequestCommandHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_Service_is_called()
        {
            // Arrange
            var request = _fixture.Create<CreateEmploymentCheckCacheRequestCommand>();

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData), Times.Once);
            _serviceMock.Verify(s => s.SaveEmploymentCheck(It.IsAny<Data.Models.EmploymentCheck>()), Times.Never);
        }

        [Test]
        public async Task Then_Check_Is_Marked_As_Complete_When_No_Valid_NiNo()
        {
            // Arrange
            var request = _fixture.Create<CreateEmploymentCheckCacheRequestCommand>();
            request.EmploymentCheckData.ApprenticeNiNumber.NiNumber = null;

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData), Times.Never);
            _serviceMock.Verify(s => s.SaveEmploymentCheck(
                It.Is<Data.Models.EmploymentCheck>(ec =>
                    ec.AccountId == request.EmploymentCheckData.EmploymentCheck.AccountId
                    && ec.Employed == request.EmploymentCheckData.EmploymentCheck.Employed
                    && ec.CorrelationId == request.EmploymentCheckData.EmploymentCheck.CorrelationId
                    && ec.Id == request.EmploymentCheckData.EmploymentCheck.Id
                    && ec.Uln == request.EmploymentCheckData.EmploymentCheck.Uln
                    && ec.ApprenticeshipId == request.EmploymentCheckData.EmploymentCheck.ApprenticeshipId
                    && ec.CheckType == request.EmploymentCheckData.EmploymentCheck.CheckType
                    && ec.MaxDate == request.EmploymentCheckData.EmploymentCheck.MaxDate
                    && ec.MinDate == request.EmploymentCheckData.EmploymentCheck.MinDate
                    && ec.RequestCompletionStatus == (int)ProcessingCompletionStatus.Completed
                )
            ), Times.Once);
        }

        [Test]
        public async Task Then_Check_Is_Marked_As_Complete_When_No_Valid_Paye_Scheme()
        {
            // Arrange
            var request = _fixture.Create<CreateEmploymentCheckCacheRequestCommand>();
            request.EmploymentCheckData.EmployerPayeSchemes = new EmployerPayeSchemes(_fixture.Create<long>());

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData), Times.Never);
            _serviceMock.Verify(s => s.SaveEmploymentCheck(
                It.Is<Data.Models.EmploymentCheck>(ec =>
                    ec.AccountId == request.EmploymentCheckData.EmploymentCheck.AccountId
                    && ec.Employed == request.EmploymentCheckData.EmploymentCheck.Employed
                    && ec.CorrelationId == request.EmploymentCheckData.EmploymentCheck.CorrelationId
                    && ec.Id == request.EmploymentCheckData.EmploymentCheck.Id
                    && ec.Uln == request.EmploymentCheckData.EmploymentCheck.Uln
                    && ec.ApprenticeshipId == request.EmploymentCheckData.EmploymentCheck.ApprenticeshipId
                    && ec.CheckType == request.EmploymentCheckData.EmploymentCheck.CheckType
                    && ec.MaxDate == request.EmploymentCheckData.EmploymentCheck.MaxDate
                    && ec.MinDate == request.EmploymentCheckData.EmploymentCheck.MinDate
                    && ec.RequestCompletionStatus == (int)ProcessingCompletionStatus.Completed
                )
            ), Times.Once);
        }
    }
}
