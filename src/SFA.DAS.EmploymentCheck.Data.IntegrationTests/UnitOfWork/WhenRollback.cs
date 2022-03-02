﻿using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.UnitOfWork
{
    public class WhenRollback : RepositoryTestBase
    {
        private IUnitOfWork _sut;
        private EmploymentCheckCacheRequest _inserted1;

        [Test]
        public async Task Can_Insert_and_Update_in_a_single_Transaction()
        {
            // Arrange
            _inserted1 = Fixture.Create<EmploymentCheckCacheRequest>();
            _sut = new Functions.Repositories.UnitOfWork(Settings);

            // Act
            await _sut.BeginAsync();
            await _sut.InsertAsync(_inserted1);
            await _sut.RollbackAsync();

            // Assert
            Get<EmploymentCheckCacheRequest>(_inserted1.Id).Result.Should().BeNull();
        }
    }
}
