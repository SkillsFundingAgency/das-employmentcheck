using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories
{
    public class WhenCheckingIsEmploymentCheckCompleted : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;

        [Test]
        public async Task Then_Returns_True_when_RequestCompletionStatus_Is_Null()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short?)2)
                .Create();
            await Insert(check);

            // Act
            var isEmploymentCheckCompleted = await _sut.IsEmploymentCheckCompleted(check.Id);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            actual.RequestCompletionStatus.Should().Be(2);
            isEmploymentCheckCompleted.Should().BeTrue();
        }

        [Test]
        public async Task Then_Returns_False_when_RequestCompletionStatus_Is_Not_Null()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short?)null)
                .Create();
            await Insert(check);

            // Act
            var isEmploymentCheckCompleted = await _sut.IsEmploymentCheckCompleted(check.Id);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            actual.RequestCompletionStatus.Should().Be(null);
            isEmploymentCheckCompleted.Should().BeFalse();

        }

        [Test]
        public async Task Then_Returns_False_when_RequestId_Is_Invalid()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings, Mock.Of<ILogger<EmploymentCheckRepository>>());

            var check = Fixture.Build<Models.EmploymentCheck>()
                .With(x => x.RequestCompletionStatus, (short?)2)
                .Create();
            await Insert(check);

            // Act
            
            var random = new Random();
            var randomId = random.Next(0, int.MaxValue);
            var isEmploymentCheckCompleted = await _sut.IsEmploymentCheckCompleted(randomId);

            // Assert
            var actual = await Get<Models.EmploymentCheck>(check.Id);

            actual.RequestCompletionStatus.Should().Be(2);
            isEmploymentCheckCompleted.Should().BeFalse();
        }
    }
}

