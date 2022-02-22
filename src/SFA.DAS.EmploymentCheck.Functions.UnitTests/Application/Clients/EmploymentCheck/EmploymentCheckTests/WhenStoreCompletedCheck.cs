﻿using System;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheck.EmploymentCheckTests
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
                Mock.Of<ILogger<IEmploymentCheckService>>(),
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

            // Act
            await _sut.StoreCompletedCheck(request, response);

            // Assert
            _employmentCheckRepositoryMock.Verify(x => x.UpdateEmploymentCheckAsComplete(request, _unitOfWorkMock.Object), Times.Once());
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
            _employmentCheckRepositoryMock
                .Setup(x => x.UpdateEmploymentCheckAsComplete(request, _unitOfWorkMock.Object))
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
            _employmentCheckRepositoryMock
                .Setup(x => x.UpdateEmploymentCheckAsComplete(request, _unitOfWorkMock.Object))
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