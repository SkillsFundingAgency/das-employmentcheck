﻿using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.StoreCompletedEmploymentCheckActivityTests
{
    public class WhenCallingStore
    {
        private Fixture _fixture;
        private Mock<ICommandDispatcher> _dispatcher;
        private EmploymentCheckData _employmentCheckData;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _dispatcher = new Mock<ICommandDispatcher>();
            _employmentCheckData = _fixture.Create<EmploymentCheckData>();
        }

        [Test]
        public async Task Then_The_Command_Was_Executed()
        {
            // Arrange
            _dispatcher.Setup(x =>
                x.Send(It.IsAny<StoreCompletedEmploymentCheckCommand>(), It.IsAny<CancellationToken>())).Verifiable();
            var sut = new StoreCompletedEmploymentCheckActivity(_dispatcher.Object);

            // Act
            await sut.Store(_employmentCheckData);

            // Assert
            _dispatcher.Verify(x => x.Send(It.Is<StoreCompletedEmploymentCheckCommand>(
                    c => c.EmploymentCheck == _employmentCheckData.EmploymentCheck),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}