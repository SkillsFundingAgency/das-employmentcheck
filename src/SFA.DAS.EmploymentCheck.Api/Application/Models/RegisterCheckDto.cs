using System;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.EmploymentCheck.Api.Application.Models
{
    public class RegisterCheckDto
    {
        [FromQuery]
        public Guid CorrelationId { get; set; }

        [FromQuery]
        public string CheckType { get; set; }

        [FromQuery]
        public long Uln { get; set; }

        [FromQuery]
        public int ApprenticeshipAccountId { get; set; }

        [FromQuery]
        public long? ApprenticeshipId { get; set; }

        [FromQuery]
        public DateTime MinDate { get; set; }

        [FromQuery]
        public DateTime MaxDate { get; set; }
    }
}