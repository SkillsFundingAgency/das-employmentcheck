using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.DataCollectionsApiClientTests
{
    public class ApiOptionRepositoryTests
    {
        [Test]
        public void When_GetTable_CloudTableIsReturned()
        {
            //Arrange
            AzureStorageConnectionConfiguration options = new AzureStorageConnectionConfiguration();
            IApiOptionsRepository sut = new ApiOptionsRepository(options);

            //Act
            var result = sut.GetOptions();

            //Assert
            result.Should().NotBeNull();

        }
    }
}
