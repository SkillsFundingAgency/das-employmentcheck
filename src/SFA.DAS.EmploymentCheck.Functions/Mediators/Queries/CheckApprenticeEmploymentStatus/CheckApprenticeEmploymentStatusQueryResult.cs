using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus
{
    public class CheckApprenticeEmploymentStatusQueryResult
    {
        public CheckApprenticeEmploymentStatusQueryResult(ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            ApprenticeEmploymentCheckMessageModel = apprenticeEmploymentCheckMessageModel;
        }

        public ApprenticeEmploymentCheckMessageModel ApprenticeEmploymentCheckMessageModel { get; }
    }
}
