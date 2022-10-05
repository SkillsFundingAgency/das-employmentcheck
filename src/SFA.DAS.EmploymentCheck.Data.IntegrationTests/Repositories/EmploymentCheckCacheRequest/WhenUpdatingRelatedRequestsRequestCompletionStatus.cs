using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheckCacheRequest
{
    public class WhenAbandonRelatedRequests : RepositoryTestBase
    {
        private IEmploymentCheckCacheRequestRepository _sut;
        private IList<Models.EmploymentCheckCacheRequest> _actual;
        private readonly Guid _correlationId = Guid.NewGuid();

        [Test]
        public async Task Then_Related_EmploymentCheckCacheRequests_Are_Updated_with_Abandoned_Status()
        {
            // Arrange
            _sut = new EmploymentCheckCacheRequestRepository(Settings, Mock.Of<IHmrcApiOptionsRepository>(), Mock.Of<ILogger<EmploymentCheckCacheRequestRepository>>());

            var testEmploymentCheckCacheRequestData = await CreateTestEmploymentCheckCacheRequestData();
            var expectedEmploymentCheckCacheRequestData = await CreateExpectedEmploymentCheckCacheRequestData();

            // Act
            foreach(var request in testEmploymentCheckCacheRequestData)
            {
                await Insert(request);
            }

            await UnitOfWorkInstance.BeginAsync();
            await _sut.AbandonRelatedRequests(testEmploymentCheckCacheRequestData.FirstOrDefault(), UnitOfWorkInstance);
            await UnitOfWorkInstance.CommitAsync();

            // Assert
            _actual = (await GetAll<Models.EmploymentCheckCacheRequest>()).OrderBy(x => x.Id).ToList();

            for (var i = 1; i < _actual.Count; i++)
            {
                var expected = expectedEmploymentCheckCacheRequestData[i];

                _actual[i].Should().BeEquivalentTo(expected,
                    opts => opts
                        .Excluding(x => x.LastUpdatedOn)
                    );
                _actual[i].LastUpdatedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            }
        }


        private async Task<IList<Models.EmploymentCheckCacheRequest>> CreateTestEmploymentCheckCacheRequestData()
        {
            return await Task.FromResult(new List<Models.EmploymentCheckCacheRequest>
            {
                new Models.EmploymentCheckCacheRequest
                {
                    Id = 1,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = _correlationId,
                    Nino = "A12345678",
                    PayeScheme = "Paye1",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = true,
                    RequestCompletionStatus = null,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Models.EmploymentCheckCacheRequest
                {
                    Id = 2,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = _correlationId,
                    Nino = "A12345678",
                    PayeScheme = "Paye2",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = null,
                    RequestCompletionStatus = null,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Models.EmploymentCheckCacheRequest
                {
                    Id = 3,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = _correlationId,
                    Nino = "A12345678",
                    PayeScheme = "Paye3",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = null,
                    RequestCompletionStatus = null,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Models.EmploymentCheckCacheRequest
                {
                    Id = 4,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = _correlationId,
                    Nino = "A12345678",
                    PayeScheme = "Paye4",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = null,
                    RequestCompletionStatus = null,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                }
            });
        }

        private async Task<IList<Models.EmploymentCheckCacheRequest>> CreateExpectedEmploymentCheckCacheRequestData()
        {
            return await Task.FromResult(new List<Models.EmploymentCheckCacheRequest>
            {
                new Models.EmploymentCheckCacheRequest
                {
                    Id = 1,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = _correlationId,
                    Nino = "A12345678",
                    PayeScheme = "Paye1",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = true,
                    RequestCompletionStatus = null,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Models.EmploymentCheckCacheRequest
                {
                    Id = 2,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = _correlationId,
                    Nino = "A12345678",
                    PayeScheme = "Paye2",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = null,
                    RequestCompletionStatus = (short)ProcessingCompletionStatus.Skipped,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Models.EmploymentCheckCacheRequest
                {
                    Id = 3,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = _correlationId,
                    Nino = "A12345678",
                    PayeScheme = "Paye3",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = null,
                    RequestCompletionStatus = (short)ProcessingCompletionStatus.Skipped,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Models.EmploymentCheckCacheRequest
                {
                    Id = 4,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = _correlationId,
                    Nino = "A12345678",
                    PayeScheme = "Paye4",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = null,
                    RequestCompletionStatus = (short)ProcessingCompletionStatus.Skipped,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                }
            });
        }
    }
}

