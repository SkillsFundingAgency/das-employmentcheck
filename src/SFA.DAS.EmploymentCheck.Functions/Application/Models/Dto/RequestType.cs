using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto
{
    public class RequestType
    {
        public long Id { get; set; }

        public string Description { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
