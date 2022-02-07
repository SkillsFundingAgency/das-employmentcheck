using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus
{
    public class SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommand
        : IRequest<SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommandResult>
    {
        public SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommand(
            Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> employmentCheckCacheRequestAndStatusToSet
        )
        {
            EmploymentCheckCacheRequestAndStatusToSet = employmentCheckCacheRequestAndStatusToSet;
        }

        public Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> EmploymentCheckCacheRequestAndStatusToSet { get; }
    }
}
