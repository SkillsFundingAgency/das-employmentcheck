using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus
{
    public class CheckApprenticeEmploymentStatusQueryResult
    {
        public CheckApprenticeEmploymentStatusQueryResult(EmploymentCheckMessage apprenticeEmploymentCheckMessageModel)
        {
            ApprenticeEmploymentCheckMessageModel = apprenticeEmploymentCheckMessageModel;
        }

        public EmploymentCheckMessage ApprenticeEmploymentCheckMessageModel { get; }
    }
}
