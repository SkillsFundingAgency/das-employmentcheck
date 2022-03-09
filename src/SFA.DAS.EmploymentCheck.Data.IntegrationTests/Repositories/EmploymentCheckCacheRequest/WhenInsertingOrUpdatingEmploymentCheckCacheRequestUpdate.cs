using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheckCacheRequest
{
    public class WhenInsertingOrUpdatingEmploymentCheckCacheRequestUpdate
        : RepositoryTestBase
    {
        private IEmploymentCheckCacheRequestRepository _sut;
        private Models.EmploymentCheckCacheRequest _actual;

        [Test]
        public async Task CanUpdate()
        {
            // Arrange
            _sut = new EmploymentCheckCacheRequestRepository(Settings, Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());
            var saved = Fixture.Create<Models.EmploymentCheckCacheRequest>();

            await Insert(saved);

            var expected = Fixture.Build<Models.EmploymentCheckCacheRequest>()
                .With(e => e.Id, saved.Id)
                .Create();

            // Act
            await _sut.Save(expected);

            // Assert
            _actual = await Get<Models.EmploymentCheckCacheRequest>(saved.Id);

            _actual.Should().BeEquivalentTo(expected);
        }

    }
}

