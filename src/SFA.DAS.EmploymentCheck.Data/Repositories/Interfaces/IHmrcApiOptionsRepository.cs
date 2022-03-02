﻿using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IHmrcApiOptionsRepository
    {
        Task<HmrcApiRateLimiterOptions> GetHmrcRateLimiterOptions();
        Task ReduceDelaySetting(HmrcApiRateLimiterOptions options);
        Task IncreaseDelaySetting(HmrcApiRateLimiterOptions options);
    }
}