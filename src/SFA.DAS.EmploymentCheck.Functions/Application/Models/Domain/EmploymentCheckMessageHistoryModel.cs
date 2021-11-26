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
            CreatedDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        }

        public EmploymentCheckMessageHistoryModel(EmploymentCheckMessageModel EmploymentCheckMessageModel)
        {
            MessageHistoryId = MessageHistoryId;
            MessageId = EmploymentCheckMessageModel.MessageId;
            EmploymentCheckId = EmploymentCheckMessageModel.EmploymentCheckId;
            CorrelationId = EmploymentCheckMessageModel.CorrelationId;
            Uln = EmploymentCheckMessageModel.Uln;
            NationalInsuranceNumber = EmploymentCheckMessageModel.NationalInsuranceNumber;
            PayeScheme = EmploymentCheckMessageModel.PayeScheme;
            MinDateTime = EmploymentCheckMessageModel.MinDateTime;
            MaxDateTime = EmploymentCheckMessageModel.MaxDateTime;
            EmploymentCheckedDateTime = EmploymentCheckMessageModel.EmploymentCheckedDateTime;
            IsEmployed = EmploymentCheckMessageModel.IsEmployed;
            ResponseId = EmploymentCheckMessageModel.ResponseId;
            ResponseMessage = EmploymentCheckMessageModel.ResponseMessage;
            MessageCreatedDateTime = EmploymentCheckMessageModel.CreatedDateTime;
            CreatedDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
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

