using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.UnitOfWork
{
    public class WhenInsertAndUpdate : RepositoryTestBase
    {
        private IUnitOfWork _sut;
        private Models.EmploymentCheck _updated;
        private EmploymentCheckCacheRequest _inserted1;
        private EmploymentCheckCacheResponse _inserted2;

        [Test]
        public async Task Can_Insert_and_Update_in_a_single_Transaction()
        {
            // Arrange
            _inserted1 = Fixture.Create<EmploymentCheckCacheRequest>();
            _inserted2 = Fixture.Create<EmploymentCheckCacheResponse>();
            _updated = Fixture.Create<Models.EmploymentCheck>();
            await Insert(_updated); // pre-existing
            
            // change everything but ID
            _updated = Fixture.Build<Models.EmploymentCheck>().With(x => x.Id, _updated.Id).Create();

            _sut = new Functions.Repositories.UnitOfWork(Settings);

            // Act
            await _sut.BeginAsync();
            await _sut.UpdateAsync(_updated);
            await _sut.InsertAsync(_inserted1);
            await _sut.InsertAsync(_inserted2);
            await _sut.CommitAsync();

            // Assert
            Get<Models.EmploymentCheck>(_updated.Id).Result.Should().BeEquivalentTo(_updated);
            Get<EmploymentCheckCacheRequest>(_inserted1.Id).Result.Should().BeEquivalentTo(_inserted1);
            Get<EmploymentCheckCacheResponse>(_inserted2.Id).Result.Should().BeEquivalentTo(_inserted2);
        }
    }
}
