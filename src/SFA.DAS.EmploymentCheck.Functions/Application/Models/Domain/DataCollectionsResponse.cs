using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class DataCollectionsResponse
    {
        public long Id { get; set; }
        public Guid CorrelationId { get; set; }
        public long Uln { get; set; }
        public string NationalInsuranceNumber { get; set; }
        public string Response { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
