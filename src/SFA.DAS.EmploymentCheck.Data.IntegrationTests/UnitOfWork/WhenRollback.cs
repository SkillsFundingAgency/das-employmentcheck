using System.Data.SqlClient;
using System.Reflection;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

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
            _sut = new Data.Repositories.UnitOfWork(Settings);

            // Act
            await _sut.BeginAsync();
            await _sut.InsertAsync(_inserted1);
            await _sut.RollbackAsync();

            // Assert
            Get<EmploymentCheckCacheRequest>(_inserted1.Id).Result.Should().BeNull();
        }

        [Test]
        public async Task Connection_is_Disposed_after_Commit()
        {
            // Arrange
            _inserted1 = Fixture.Create<EmploymentCheckCacheRequest>();
            _sut = new Data.Repositories.UnitOfWork(Settings);
            var internalSqlConnectionField = typeof(Data.Repositories.UnitOfWork).GetField("_sqlConnection", BindingFlags.NonPublic | BindingFlags.Instance);
            internalSqlConnectionField.Should().NotBeNull();
            // Act
            await _sut.BeginAsync();
            
            // Assert
            internalSqlConnectionField.GetValue(_sut).Should().NotBeNull();

            // Act
            await _sut.InsertAsync(_inserted1);
            await _sut.CommitAsync();
            
            // Act
            internalSqlConnectionField.GetValue(_sut).Should().BeNull();
        }

        [Test]
        public async Task Connection_is_Disposed_after_Rollback()
        {
            // Arrange
            _inserted1 = Fixture.Create<EmploymentCheckCacheRequest>();
            _sut = new Data.Repositories.UnitOfWork(Settings);
            var internalSqlConnectionField = typeof(Data.Repositories.UnitOfWork).GetField("_sqlConnection", BindingFlags.NonPublic | BindingFlags.Instance);
            internalSqlConnectionField.Should().NotBeNull();
            // Act
            await _sut.BeginAsync();
            
            // Assert
            internalSqlConnectionField.GetValue(_sut).Should().NotBeNull();

            // Act
            await _sut.InsertAsync(_inserted1);
            await _sut.RollbackAsync();
            
            // Act
            internalSqlConnectionField.GetValue(_sut).Should().BeNull();
        }
    }
}
