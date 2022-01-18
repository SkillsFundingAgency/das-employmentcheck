using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Repositories;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Tests.Repositories
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
                .With(x => x.VersionId, 1)
                .Create();

            var second = Fixture.Build<Api.Application.Models.EmploymentCheck>()
                .With(x => x.CorrelationId, correlationId)
                .With(x => x.VersionId, 2)
                .Create();

            var last = Fixture.Build<Api.Application.Models.EmploymentCheck>()
                .With(x => x.CorrelationId, correlationId)
                .With(x => x.VersionId, 3)
                .Create();

            var irrelevant = Fixture.Build<Api.Application.Models.EmploymentCheck>()
                .With(x => x.CorrelationId, Guid.NewGuid())
                .With(x => x.VersionId, 4)
                .Create();

            await Insert(first);
            await Insert(second);
            await Insert(last);
            await Insert(irrelevant);

            //Act

            var actual = await _sut.GetEmploymentCheck(correlationId);

            //Assert

            actual.Should().BeEquivalentTo(last,
                opts => opts
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
            );
            actual.MinDate.Should().BeCloseTo(last.MinDate, TimeSpan.FromSeconds(1));
            actual.MaxDate.Should().BeCloseTo(last.MaxDate, TimeSpan.FromSeconds(1));
            actual.CreatedOn.Should().BeCloseTo(last.CreatedOn, TimeSpan.FromSeconds(1));
            actual.LastUpdatedOn.Should().BeCloseTo(last.LastUpdatedOn, TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task Then_Default_Check_Is_Returned_If_Not_Exists()
        {
            //Arrange

            _sut = new EmploymentCheckRepository(Settings);

            var correlationId = Guid.NewGuid();

            var irrelevant = Fixture.Build<Api.Application.Models.EmploymentCheck>()
                .With(x => x.CorrelationId, Guid.NewGuid())
                .With(x => x.VersionId, 4)
                .Create();

            await Insert(irrelevant);

            //Act

            var actual = await _sut.GetEmploymentCheck(correlationId);

            //Assert
            actual.Should().BeNull();
        }
    }
}