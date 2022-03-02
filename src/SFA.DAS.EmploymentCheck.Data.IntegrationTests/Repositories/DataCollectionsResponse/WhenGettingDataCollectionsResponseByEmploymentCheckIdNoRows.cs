using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.DataCollectionsResponse
{
    public class WhenGettingDataCollectionsResponseByEmploymentCheckIdNoRows
        : RepositoryTestBase
    {
        private IDataCollectionsResponseRepository _sut;
        private Models.DataCollectionsResponse _actual;

        [Test]
        public async Task Get_Should_Return_Null_When_No_Row_Found_For_The_Given_ApprenticeEmploymentCheckId()
        {
            // Arrange
            _sut = new DataCollectionsResponseRepository(Settings);

            // Act
            _actual = await _sut.GetByEmploymentCheckId(-1);

            // Assert
            _actual.Should().BeNull();
        }
    }
}

