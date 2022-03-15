﻿using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests.PublishEmploymentCheckResult
{
    public class WhenHandlingTheRequest
    {
        private PublishEmploymentCheckResultCommandHandler _sut;
        private Mock<ICommandService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<ICommandService>();
            _sut = new PublishEmploymentCheckResultCommandHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_a_message_is_published()
        {
            // Arrange
            var request = _fixture.Create<PublishEmploymentCheckResultCommand>();

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(_ => _.Dispatch(request), Times.Once);
        }
    }
}
