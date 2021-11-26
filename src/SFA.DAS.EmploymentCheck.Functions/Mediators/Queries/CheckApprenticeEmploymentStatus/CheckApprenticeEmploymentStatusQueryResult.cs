using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus
{
    public class CheckApprenticeEmploymentStatusQueryResult
    {
        public CheckApprenticeEmploymentStatusQueryResult(EmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            ApprenticeEmploymentCheckMessageModel = apprenticeEmploymentCheckMessageModel;
        }

        public EmploymentCheckMessageModel ApprenticeEmploymentCheckMessageModel { get; }
    }
}
