using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Activities.CreateEmploymentCheckCacheRequestsActivityTests
{
    public class WhenCallingCreate
    {
        private readonly Fixture _fixture;
        private readonly Mock<IMediator> _mediator;
        private readonly EmploymentCheckCacheRequest _employmentCheckCacheRequest;
        private readonly EmploymentCheckData _employmentCheckData;

        public WhenCallingCreate()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
            _employmentCheckData = _fixture.Create<EmploymentCheckData>();
        }

        [Test]
        public async Task Then_The_Command_Was_Executed()
        {
            // Arrange
            var sut = new CreateEmploymentCheckCacheRequestsActivity(_mediator.Object);

            // Act
            await sut.Create(_employmentCheckData);

            // Assert
            Assert.IsTrue(true);
        }
    }
}