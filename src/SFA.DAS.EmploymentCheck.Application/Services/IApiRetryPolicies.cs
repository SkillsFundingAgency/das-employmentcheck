﻿using System;
using System.Threading.Tasks;
using Polly;
using Polly.Wrap;

namespace SFA.DAS.EmploymentCheck.Application.Services
{
    public interface IApiRetryPolicies
    {
        Task<AsyncPolicyWrap> GetAll(Func<Task> onRetry);
        //Task ReduceRetryDelay();
        Task<AsyncPolicy> GetRetrievalRetryPolicy();
    }
}
