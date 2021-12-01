using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckEmploymentStatus
{
    public class CheckEmploymentStatusQueryRequest
        : IRequest<CheckEmploymentStatusQueryResult>
    {
        public CheckEmploymentStatusQueryRequest(
            EmploymentCheckMessage employmentCheckMessage)
        {
            EmploymentCheckMessage = employmentCheckMessage;
        }

        public EmploymentCheckMessage EmploymentCheckMessage { get; }
    }
}
