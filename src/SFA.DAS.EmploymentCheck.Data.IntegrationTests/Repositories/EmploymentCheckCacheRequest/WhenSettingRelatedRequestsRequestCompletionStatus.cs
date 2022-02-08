using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.IntegrationTests.Repositories.EmploymentCheckCacheRequest
{
    public class WhenSettingRelatedRequestsRequestCompletionStatus
        : RepositoryTestBase
    {

        private IEmploymentCheckCacheRequestRepository _sut;
        private IList<Models.EmploymentCheckCacheRequest> _actual;

        [Test]
        public async Task CanSave()
        {
            // Arrange
            _sut = new EmploymentCheckCacheRequestRepository(Settings);

            var testEmploymentCheckCacheRequestData = await CreateTestEmploymentCheckCacheRequestData();
            var expectedEmploymentCheckCacheRequestData = await CreateExpectedEmploymentCheckCacheRequestData();

            // Act
            foreach (var request in testEmploymentCheckCacheRequestData)
            {
                await base.Insert(request);
            }

            var result = new Tuple<Models.EmploymentCheckCacheRequest, ProcessingCompletionStatus>(testEmploymentCheckCacheRequestData.FirstOrDefault(), ProcessingCompletionStatus.Skipped);
            await _sut.SetRelatedRequestsCompletionStatus(result);

            // Assert
            _actual = (await GetAll<Models.EmploymentCheckCacheRequest>()).OrderBy(x => x.Id).ToList();

            for (var i = 1; i < _actual.Count; i++)
            {
                var expected = expectedEmploymentCheckCacheRequestData[i];
                var actual = _actual[i];

                actual.Should().BeEquivalentTo(expected,
                    opts => opts
                        .Excluding(x => x.Id) // The Id is the Identity Column from the db so ignore
                        .Excluding(x => x.LastUpdatedOn)
                        .Excluding(x => x.CreatedOn));
            }
        }

        [TearDown]
        public async Task CleanUp()
        {
            await Delete(_actual);
        }

        // TODO: Switch to use Build
        private async Task<IList<Functions.Application.Models.EmploymentCheckCacheRequest>> CreateTestEmploymentCheckCacheRequestData()
        {
            return await Task.FromResult(new List<Functions.Application.Models.EmploymentCheckCacheRequest>
            {
                new Models.EmploymentCheckCacheRequest
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
                    RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                },
                new Models.EmploymentCheckCacheRequest
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
                new Models.EmploymentCheckCacheRequest
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
                new Models.EmploymentCheckCacheRequest
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
                    RequestCompletionStatus = (short)ProcessingCompletionStatus.Skipped,
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
                    RequestCompletionStatus = (short)ProcessingCompletionStatus.Skipped,
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
                    RequestCompletionStatus = (short)ProcessingCompletionStatus.Skipped,
                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
                }
            });
        }
    }
}

