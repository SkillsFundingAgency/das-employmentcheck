using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models
{
    [Table("Learner", Schema = "incentives")]
    public class Learner
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public long ApprenticeshipId { get; set; }
        public long Ukprn { get; set; }
        public long ULN { get; set; }
        public bool SubmissionFound { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public bool? LearningFound { get; set; }
        public bool? HasDataLock { get; set; }
        public DateTime? StartDate { get; set; }
        public bool? InLearning { get; set; }
        public DateTime? LearningStoppedDate { get; set; }
        public DateTime? LearningResumedDate { get; set; }
        public bool SuccessfulLearnerMatchExecution { get; set; }
        public string RawJSON { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
