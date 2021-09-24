using System;
using SFA.DAS.Common.Domain.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models
{
    [Table("ApprenticeshipDaysInLearning", Schema = "incentives")]
    public partial class ApprenticeshipDaysInLearning
    {
        public Guid LearnerId { get; set; }
        public int NumberOfDaysInLearning { get; set; }
        public byte CollectionPeriodNumber { get; set; }
        public short CollectionPeriodYear { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
