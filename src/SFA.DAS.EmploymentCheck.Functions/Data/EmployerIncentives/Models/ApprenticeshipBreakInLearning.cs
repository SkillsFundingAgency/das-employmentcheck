using System;
using SFA.DAS.Common.Domain.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models
{
    [Table("ApprenticeshipBreakInLearning", Schema = "incentives")]
    public partial class ApprenticeshipBreakInLearning
    {
        public Guid ApprenticeshipIncentiveId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
