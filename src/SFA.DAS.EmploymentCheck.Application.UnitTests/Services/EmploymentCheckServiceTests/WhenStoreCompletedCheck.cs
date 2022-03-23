using System;
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
    public class WhenStoreCompletedCheck
    {
        private IEmploymentCheckService _sut;
        private Fixture _fixture;
        private Mock<IEmploymentCheckRepository> _employmentCheckRepositoryMock;
        private Mock<IEmploymentCheckCacheRequestRepository> _employmentCheckCacheRequestRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckRepositoryMock = new Mock<IEmploymentCheckRepository>();
            _employmentCheckCacheRequestRepositoryMock = new Mock<IEmploymentCheckCacheRequestRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _sut = new EmploymentCheckService(
                _employmentCheckRepositoryMock.Object,
                _employmentCheckCacheRequestRepositoryMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Test]
        public async Task Then_Request_is_Updated()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCacheRequest>();
            var response = _fixture.Create<EmploymentCheckCacheResponse>();

            // Act
            await _sut.StoreCompletedCheck(request, response);

            // Assert
            _unitOfWorkMock.Verify(x => x.UpdateAsync(request), Times.Once());
        }

        [Test]
        public async Task Then_Response_is_Inserted()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCacheRequest>();
            var response = _fixture.Create<EmploymentCheckCacheResponse>();

            // Act
            await _sut.StoreCompletedCheck(request, response);

            // Assert
            _unitOfWorkMock.Verify(x => x.InsertAsync(response), Times.Once());
        }

        [Test]
        public async Task Then_UpdateEmploymentCheckAsComplete_is_called()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCacheRequest>();
            var response = _fixture.Create<EmploymentCheckCacheResponse>();
            var employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>()
                .With(x => x.Id, request.ApprenticeEmploymentCheckId)
                .With(x => x.Employed, request.Employed)
                .With(x => x.RequestCompletionStatus, request.RequestCompletionStatus)
                .Without(x => x.ErrorType)
                .Create();

            _employmentCheckRepositoryMock
                .Setup(x => x.UpdateEmploymentCheckAsComplete(employmentCheck, _unitOfWorkMock.Object))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.StoreCompletedCheck(request, response);

            // Assert
            _employmentCheckRepositoryMock.Verify(x => x.UpdateEmploymentCheckAsComplete(
                It.Is<Data.Models.EmploymentCheck>(
                    x => x.Id == request.ApprenticeEmploymentCheckId
                    && x.Employed == request.Employed
                    && x.RequestCompletionStatus == request.RequestCompletionStatus
                    && x.ErrorType == null
                    )
                ,_unitOfWorkMock.Object)
                ,Times.Once());
        }

        [Test]
        public async Task Then_Unit_of_Work_Transaction_Is_Committed()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCacheRequest>();
            var response = _fixture.Create<EmploymentCheckCacheResponse>();

            // Act
            await _sut.StoreCompletedCheck(request, response);

            // Assert
            _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Once());
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once());
        }

        [Test]
        public void Then_Unit_of_Work_Transaction_Is_Rolledback_in_case_of_error()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCacheRequest>();
            var response = _fixture.Create<EmploymentCheckCacheResponse>();

            _unitOfWorkMock
                .Setup(x => x.UpdateAsync(request))
                .Throws(new Exception());

            // Act
            _sut.StoreCompletedCheck(request, response);

            // Assert
            _unitOfWorkMock.Verify(x => x.BeginAsync(), Times.Once());
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never());
            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once());
        }

        [Test]
        public void Then_Exception_is_rethrown_in_case_of_error()
        {
            // Arrange
            var exception = new InvalidOperationException(_fixture.Create<string>());
            var request = _fixture.Create<EmploymentCheckCacheRequest>();
            var response = _fixture.Create<EmploymentCheckCacheResponse>();
            var check = _fixture.Create<Data.Models.EmploymentCheck>();
            _employmentCheckRepositoryMock
                .Setup(x => x.UpdateEmploymentCheckAsComplete(check, _unitOfWorkMock.Object))
                .Throws(exception);

            // Act
            Func<Task> act = () => _sut.StoreCompletedCheck(request, response);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>().WithMessage(exception.Message);
        }

        [Test]
        public async Task Then_SetRelatedRequestsRequestCompletionStatus_is_called_When_result_is_Employed()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCacheRequest>();
            var response = _fixture.Build<EmploymentCheckCacheResponse>().With(x => x.Employed, true).Create();

            // Act
            await _sut.StoreCompletedCheck(request, response);

            // Assert
            _employmentCheckCacheRequestRepositoryMock.Verify(x => x.AbandonRelatedRequests(
                request, _unitOfWorkMock.Object), Times.Once());
        }

        [Test]
        public async Task Then_SetRelatedRequestsRequestCompletionStatus_is_not_called_When_result_is_not_Employed()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCacheRequest>();
            var response = _fixture.Build<EmploymentCheckCacheResponse>().With(x => x.Employed, false).Create();

            // Act
            await _sut.StoreCompletedCheck(request, response);

            // Assert
            _employmentCheckCacheRequestRepositoryMock.Verify(x => x.AbandonRelatedRequests(
                request, _unitOfWorkMock.Object), Times.Never());
        }

    }
}
