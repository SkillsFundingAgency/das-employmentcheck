using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Repositories;

namespace SFA.DAS.EmploymentCheck.Api.UnitTests.Repositories
{
    public class WhenGettingEmploymentCheck : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;

        [Test]
        public async Task Then_The_Check_Is_Returned_If_Exists()
        {
            //Arrange

            _sut = new EmploymentCheckRepository(Settings);

            var correlationId = Guid.NewGuid();

            var first = Fixture.Build<Api.Application.Models.EmploymentCheck>()
                .With(x => x.CorrelationId, correlationId)
                .Create();

            var second = Fixture.Build<Api.Application.Models.EmploymentCheck>()
                .With(x => x.CorrelationId, correlationId)
                .Create();

            var last = Fixture.Build<Api.Application.Models.EmploymentCheck>()
                .With(x => x.CorrelationId, correlationId)
                .Create();

            var irrelevant = Fixture.Build<Api.Application.Models.EmploymentCheck>()
                .With(x => x.CorrelationId, Guid.NewGuid())
                .Create();

            await Insert(first);
            await Insert(second);
            await Insert(last);
            await Insert(irrelevant);

            //Act

            var actual = await _sut.GetEmploymentCheck(correlationId);

            //Assert

            actual.Should().BeEquivalentTo(first,
                opts => opts
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
            );
            actual.MinDate.Should().BeCloseTo(first.MinDate, TimeSpan.FromSeconds(1));
            actual.MaxDate.Should().BeCloseTo(first.MaxDate, TimeSpan.FromSeconds(1));
            actual.CreatedOn.Should().BeCloseTo(first.CreatedOn, TimeSpan.FromSeconds(1));
            actual.LastUpdatedOn.Should().BeCloseTo(first.LastUpdatedOn, TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task Then_Null_Is_Returned_If_Not_Exists()
        {
            //Arrange

            _sut = new EmploymentCheckRepository(Settings);

            var correlationId = Guid.NewGuid();

            var irrelevant = Fixture.Build<Api.Application.Models.EmploymentCheck>()
                .With(x => x.CorrelationId, Guid.NewGuid())
                .Create();

            await Insert(irrelevant);

            //Act

            var actual = await _sut.GetEmploymentCheck(correlationId);

            //Assert
            actual.Should().BeNull();
        }
    }
}