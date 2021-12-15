using MediatR;
using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests
{
    public class CreateEmploymentCheckCacheRequestsCommandRequest
        : IRequest<CreateEmploymentCheckCacheRequestsCommandResult>
    {
        public CreateEmploymentCheckCacheRequestsCommandRequest(
            IList<EmploymentCheckModel> employmentCheckModels)
        {
            EmploymentCheckModels = employmentCheckModels;
        }

        public IList<EmploymentCheckModel> EmploymentCheckModels { get; }
    }
}