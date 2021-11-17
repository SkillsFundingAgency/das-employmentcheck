using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus
{
    public class CheckApprenticeEmploymentStatusQueryRequest
        : IRequest<CheckApprenticeEmploymentStatusQueryResult>
    {
        public CheckApprenticeEmploymentStatusQueryRequest(
            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            ApprenticeEmploymentCheckMessageModel = apprenticeEmploymentCheckMessageModel;
        }

        public ApprenticeEmploymentCheckMessageModel ApprenticeEmploymentCheckMessageModel { get; }
    }
}
