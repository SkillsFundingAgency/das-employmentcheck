using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Commands.CreateEmploymentCheckCacheRequests.HandlerTests
{
    public class WhenHandlingTheRequest
    {
        private Fixture _fixture;
        private Mock<IEmploymentCheckClient> _employmentCheckClient;
        private Mock<IEmploymentCheckCacheRequestFactory> _cacheRequestFactory;
        private EmploymentCheckData _employmentCheckData;
        private IList<EmploymentCheckCacheRequest> _employmentCheckCacheRequests;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckClient = new Mock<IEmploymentCheckClient>();
            _employmentCheckData = _fixture.Create<EmploymentCheckData>();
            _employmentCheckCacheRequests = new List<EmploymentCheckCacheRequest> { _fixture.Create<EmploymentCheckCacheRequest>() };
            _cacheRequestFactory = new Mock<IEmploymentCheckCacheRequestFactory>();
        }

        //[Test]
        public async Task Then_The_EmploymentCheckClient_Is_Called()
        {
            // Arrange
            var command = new CreateEmploymentCheckCacheRequestCommand(_employmentCheckData);

            _employmentCheckClient.Setup(x => x.CreateEmploymentCheckCacheRequests(command.EmploymentCheckData, _cacheRequestFactory.Object))
                .ReturnsAsync(new List<EmploymentCheckCacheRequest>());

            var sut = new CreateEmploymentCheckCacheRequestCommandHandler(_employmentCheckClient.Object, _cacheRequestFactory.Object);

            // Act
            await sut.Handle(new CreateEmploymentCheckCacheRequestCommand(_employmentCheckData), CancellationToken.None);

            // Assert
            _employmentCheckClient.Verify(x => x.CreateEmploymentCheckCacheRequests(command.EmploymentCheckData, _cacheRequestFactory.Object), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_EmploymentCheckClient_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            // Arrange
            var command = new CreateEmploymentCheckCacheRequestCommand(_employmentCheckData);

            _employmentCheckClient.Setup(x => x.CreateEmploymentCheckCacheRequests(command.EmploymentCheckData, _cacheRequestFactory.Object))
                .ReturnsAsync((List<EmploymentCheckCacheRequest>)null);

            var sut = new CreateEmploymentCheckCacheRequestCommandHandler(_employmentCheckClient.Object, _cacheRequestFactory.Object);

            // Act
            var result = await sut.Handle(new CreateEmploymentCheckCacheRequestCommand(_employmentCheckData), CancellationToken.None);

            // Assert
            result.EmploymentCheckCacheRequest.Should().BeEquivalentTo(new EmploymentCheckCacheRequest());
        }

        [Test]
        public async Task And_The_EmploymentCheckClient_Returns_EmploymentCheckCacheRequests_Then_They_Are_Returned()
        {
            // Arrange
            var command = new CreateEmploymentCheckCacheRequestCommand(_employmentCheckData);

            _employmentCheckClient.Setup(x => x.CreateEmploymentCheckCacheRequests(command.EmploymentCheckData, _cacheRequestFactory.Object))
                .ReturnsAsync(_employmentCheckCacheRequests);

            var sut = new CreateEmploymentCheckCacheRequestCommandHandler(_employmentCheckClient.Object, _cacheRequestFactory.Object);

            // Act
            var result = await sut.Handle(new CreateEmploymentCheckCacheRequestCommand(_employmentCheckData), CancellationToken.None);

            // Assert
            result.EmploymentCheckCacheRequest.Should().BeEquivalentTo(_employmentCheckCacheRequests.FirstOrDefault());
        }
    }
}