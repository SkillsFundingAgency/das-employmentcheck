using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto
{
    public class DataCollectionsApiResult
    {
        public ApprenticeNiNumber apprenticeNiNumber { get; set; }

        public bool NiNumberFound { get; set; }

        public EmploymentCheckCacheRequest employmentCheckCacheRequest { get; set; }
    }
}
