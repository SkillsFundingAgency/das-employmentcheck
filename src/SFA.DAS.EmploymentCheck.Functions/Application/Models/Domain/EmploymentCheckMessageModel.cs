using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class EmploymentCheckMessageModel
    {
        public EmploymentCheckMessageModel() { }

        public EmploymentCheckMessageModel(
            long messageId,
            long employmentCheckId,
            long correlationId,
            long uln,
            string nationalInsuranceNumber,
            string payeScheme,
            DateTime minDateTime,
            DateTime maxDateTime,
            bool? isEmployed,
            DateTime? employmentCheckedDateTime,
            short responseId,
            string responseMessage,
            DateTime createdDateTime)
        {
            MessageId = messageId;
            EmploymentCheckId = employmentCheckId;
            CorrelationId = correlationId;
            Uln = uln;
            NationalInsuranceNumber = nationalInsuranceNumber;
            PayeScheme = payeScheme;
            MinDateTime = minDateTime;
            MinDateTime = maxDateTime;
            IsEmployed = isEmployed;
            EmploymentCheckedDateTime = employmentCheckedDateTime;
            ResponseId = responseId;
            ResponseMessage = responseMessage;
            CreatedDateTime = createdDateTime;
        }

        public long MessageId { get; set; }

        public long EmploymentCheckId { get; set; }

        public long CorrelationId { get; set; }

        public long Uln { get; set; }

        public string NationalInsuranceNumber { get; set; }

        public string PayeScheme { get; set; }

        public DateTime MinDateTime { get; set; }

        public DateTime MaxDateTime { get; set; }

        public bool? IsEmployed { get; set; }

        public DateTime? EmploymentCheckedDateTime { get; set; }

        public short ResponseId { get; set; }

        public string ResponseMessage { get; set; }

        public DateTime CreatedDateTime { get; set; }
    }
}

