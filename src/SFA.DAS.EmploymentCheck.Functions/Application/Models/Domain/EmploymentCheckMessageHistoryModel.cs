using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class EmploymentCheckMessageHistoryModel
    {
        public EmploymentCheckMessageHistoryModel() { }

        public EmploymentCheckMessageHistoryModel(
            long messageHistoryId,
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
            short responeId,
            string responseMessage,
            DateTime messageCreatedDateTime,
            DateTime createdDateTime)
        {
            MessageHistoryId = messageHistoryId;
            MessageId = messageId;
            EmploymentCheckId = employmentCheckId;
            CorrelationId = correlationId;
            Uln = uln;
            NationalInsuranceNumber = nationalInsuranceNumber;
            PayeScheme = payeScheme;
            MinDateTime = minDateTime;
            MaxDateTime = maxDateTime;
            EmploymentCheckedDateTime = employmentCheckedDateTime;
            IsEmployed = isEmployed;
            ResponseId = responeId;
            ResponseMessage = responseMessage;
            MessageCreatedDateTime = messageCreatedDateTime;
            CreatedDateTime = DateTime.UtcNow;
        }

        public EmploymentCheckMessageHistoryModel(EmploymentCheckMessageModel employmentCheckMessageModel)
        {
            MessageId = employmentCheckMessageModel.MessageId;
            EmploymentCheckId = employmentCheckMessageModel.EmploymentCheckId;
            CorrelationId = employmentCheckMessageModel.CorrelationId;
            Uln = employmentCheckMessageModel.Uln;
            NationalInsuranceNumber = employmentCheckMessageModel.NationalInsuranceNumber;
            PayeScheme = employmentCheckMessageModel.PayeScheme;
            MinDateTime = employmentCheckMessageModel.MinDateTime;
            MaxDateTime = employmentCheckMessageModel.MaxDateTime;
            EmploymentCheckedDateTime = employmentCheckMessageModel.EmploymentCheckedDateTime;
            IsEmployed = employmentCheckMessageModel.IsEmployed;
            ResponseId = employmentCheckMessageModel.ResponseId;
            ResponseMessage = employmentCheckMessageModel.ResponseMessage;
            MessageCreatedDateTime = employmentCheckMessageModel.CreatedDateTime;
            CreatedDateTime = DateTime.UtcNow;
        }

        // Message history properties
        public long MessageHistoryId { get; set; }

        // Employment check message properties
        public long MessageId { get; set; }

        public long CorrelationId { get; set; }

        public long EmploymentCheckId { get; set; }

        public long Uln { get; set; }

        public string NationalInsuranceNumber { get; set; }

        public string PayeScheme { get; set; }

        public DateTime MinDateTime { get; set; }

        public DateTime MaxDateTime { get; set; }

        public DateTime? EmploymentCheckedDateTime { get; set; }

        public bool? IsEmployed { get; set; }

        public short ResponseId { get; set; }

        public string ResponseMessage { get; set; }

        public DateTime MessageCreatedDateTime { get; set; }

        public DateTime CreatedDateTime { get; set; }

    }
}

