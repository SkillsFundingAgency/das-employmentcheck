using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus
{
    public class CheckApprenticeEmploymentStatusQueryRequest
        : IRequest<CheckApprenticeEmploymentStatusQueryResult>
    {
        public CheckApprenticeEmploymentStatusQueryRequest(
            EmploymentCheckMessage apprenticeEmploymentCheckMessageModel)
        {
            ApprenticeEmploymentCheckMessageModel = apprenticeEmploymentCheckMessageModel;
        }

        public EmploymentCheckMessage ApprenticeEmploymentCheckMessageModel { get; }
    }
}
