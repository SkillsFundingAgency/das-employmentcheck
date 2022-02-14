using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheckCacheRequest
{
    public class WhenUpdatingRelatedRequestsRequestCompletionStatus
        : RepositoryTestBase
    {
        private IEmploymentCheckCacheRequestRepository _sut;
        private IList<Functions.Application.Models.EmploymentCheckCacheRequest> _actual;

        [Test]
        public async Task CanSave()
        {
            // Arrange
            _sut = new EmploymentCheckCacheRequestRepository(Settings);

            var testEmploymentCheckCacheRequestData = await CreateTestEmploymentCheckCacheRequestData();
            var expectedEmploymentCheckCacheRequestData = await CreateExpectedEmploymentCheckCacheRequestData();

            // Act
            foreach(var request in testEmploymentCheckCacheRequestData)
            {
                await Insert(request);
            }

            await _sut.SetRelatedRequestsRequestCompletionStatus(testEmploymentCheckCacheRequestData.FirstOrDefault(), ProcessingCompletionStatus.Abandoned);

            // Assert
            _actual = (await GetAll<Functions.Application.Models.EmploymentCheckCacheRequest>()).OrderBy(x => x.Id).ToList();

            for (var i = 1; i < _actual.Count; i++)
            {
                var expected = expectedEmploymentCheckCacheRequestData[i];

                _actual[i].Should().BeEquivalentTo(expected,
                    opts => opts
                        .Excluding(x => x.Id)
                        .Excluding(x => x.PayeScheme)
                        .Excluding(x => x.LastUpdatedOn)
                        .Excluding(x => x.CreatedOn));
                _actual[i].CreatedOn.Should().BeCloseTo(expected.CreatedOn, TimeSpan.FromSeconds(1));
                _actual[i].Id.Should().BeGreaterThan(0);
            }
        }

        [TearDown]
        public async Task CleanUp()
        {
            await Delete(_actual);
        }

        private async Task<IList<Functions.Application.Models.EmploymentCheckCacheRequest>> CreateTestEmploymentCheckCacheRequestData()
        {
            return await Task.FromResult(new List<Functions.Application.Models.EmploymentCheckCacheRequest>
            {
                new Functions.Application.Models.EmploymentCheckCacheRequest
                {
                    Id = 1,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
                    Nino = "A12345678",
                    PayeScheme = "Paye1",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = true,
                    RequestCompletionStatus = null,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Functions.Application.Models.EmploymentCheckCacheRequest
                {
                    Id = 2,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
                    Nino = "A12345678",
                    PayeScheme = "Paye2",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = null,
                    RequestCompletionStatus = null,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Functions.Application.Models.EmploymentCheckCacheRequest
                {
                    Id = 3,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
                    Nino = "A12345678",
                    PayeScheme = "Paye3",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = null,
                    RequestCompletionStatus = null,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Functions.Application.Models.EmploymentCheckCacheRequest
                {
                    Id = 4,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
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

        private async Task<IList<Functions.Application.Models.EmploymentCheckCacheRequest>> CreateExpectedEmploymentCheckCacheRequestData()
        {
            return await Task.FromResult(new List<Functions.Application.Models.EmploymentCheckCacheRequest>
            {
                new Functions.Application.Models.EmploymentCheckCacheRequest
                {
                    Id = 1,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
                    Nino = "A12345678",
                    PayeScheme = "Paye1",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = true,
                    RequestCompletionStatus = null,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Functions.Application.Models.EmploymentCheckCacheRequest
                {
                    Id = 2,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
                    Nino = "A12345678",
                    PayeScheme = "Paye2",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = null,
                    RequestCompletionStatus = (short)ProcessingCompletionStatus.Abandoned,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Functions.Application.Models.EmploymentCheckCacheRequest
                {
                    Id = 3,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
                    Nino = "A12345678",
                    PayeScheme = "Paye3",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = null,
                    RequestCompletionStatus = (short)ProcessingCompletionStatus.Abandoned,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Functions.Application.Models.EmploymentCheckCacheRequest
                {
                    Id = 4,
                    ApprenticeEmploymentCheckId = 1,
                    CorrelationId = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
                    Nino = "A12345678",
                    PayeScheme = "Paye4",
                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                    Employed = null,
                    RequestCompletionStatus = (short)ProcessingCompletionStatus.Abandoned,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                }
            });
        }
    }
}

