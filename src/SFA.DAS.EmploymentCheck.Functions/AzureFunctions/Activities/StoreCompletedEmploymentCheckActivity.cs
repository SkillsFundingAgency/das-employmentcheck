﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Commands;
using SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class StoreCompletedEmploymentCheckActivity
    {
        private readonly ICommandDispatcher _dispatcher;

        public StoreCompletedEmploymentCheckActivity(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [FunctionName(nameof(StoreCompletedEmploymentCheckActivity))]
        public async Task Store([ActivityTrigger] Data.Models.EmploymentCheck employmentCheck)
        {
            await _dispatcher.Send(new StoreCompletedEmploymentCheckCommand(employmentCheck));
        }
    }
}