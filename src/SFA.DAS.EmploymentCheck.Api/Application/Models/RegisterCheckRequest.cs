using System;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.EmploymentCheck.Api.Application.Models
{
    public class RegisterCheckRequest
    {
        public Guid CorrelationId { get; set; } 
        public string CheckType { get; set; }
        public long Uln { get; set; }
        public int ApprenticeshipAccountId { get; set; }
        public long? ApprenticeshipId { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
    }
}