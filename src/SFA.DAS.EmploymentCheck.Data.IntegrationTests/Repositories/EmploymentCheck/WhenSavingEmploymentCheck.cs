﻿using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheck
{
    public class WhenSavingEmploymentCheck
        : RepositoryTestBase
    {
        private IEmploymentCheckRepository _sut;
        private Models.EmploymentCheck _actual;

        [Test]
        public async Task CanSave()
        {
            // Arrange
            _sut = new EmploymentCheckRepository(Settings);
            var expected = Fixture.Create<Models.EmploymentCheck>();

            // Act
            await _sut.Save(expected);

            // Assert
            _actual = (await GetAll<Models.EmploymentCheck>())
                .Single(x => x.Id == expected.Id); // Note: The Id is the primary key on this table which is 'unique' for this row, the CorrelationId is can be considered as the Id of a group of rows and may not be 'unique' for this row

            _actual.Should().BeEquivalentTo(expected,
                opts => opts
                    .Excluding(x => x.Id)
                    .Excluding(x => x.CreatedOn)
                    .Excluding(x => x.LastUpdatedOn)
                    .Excluding(x => x.MinDate)
                    .Excluding(x => x.MaxDate)
                );

            _actual.CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
            _actual.LastUpdatedOn.Should().BeCloseTo(expected.LastUpdatedOn.Value, TimeSpan.FromSeconds(1));
            _actual.MinDate.Should().BeCloseTo(expected.MinDate, TimeSpan.FromSeconds(1));
            _actual.MaxDate.Should().BeCloseTo(expected.MaxDate, TimeSpan.FromSeconds(1));
            _actual.Id.Should().BeGreaterThan(0);
        }

        [TearDown]
        public async Task CleanUp()
        {
            await Delete(_actual);
        }
    }
}
