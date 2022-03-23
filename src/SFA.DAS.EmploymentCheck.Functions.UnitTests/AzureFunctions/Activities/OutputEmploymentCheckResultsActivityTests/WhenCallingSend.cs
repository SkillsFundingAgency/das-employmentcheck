using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.OutputEmploymentCheckResultsActivityTests
{
    public class WhenCallingSend
    {
        private Fixture _fixture;
        private Data.Models.EmploymentCheck _employmentCheck;
        private Mock<ICommandDispatcher> _dispatcher;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _dispatcher = new Mock<ICommandDispatcher>();
            _employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().With(x => x.RequestCompletionStatus, (short)ProcessingCompletionStatus.Completed).Create();
        }

        [Test]
        public async Task Then_The_EmploymentChecks_Are_Returned()
        {
            // Arrange
            var sut = new OutputEmploymentCheckResultsActivity(_dispatcher.Object);

            // Act
            await sut.Send(_employmentCheck);

            // Assert
            _dispatcher.Verify(d =>
                d.Send(It.Is<EmploymentCheckCompletedEvent>(e => e.EmploymentCheck == _employmentCheck),
                    CancellationToken.None), Times.Once());
        }
    }
}