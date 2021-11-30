using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class ApprenticeEmploymentCheckMessageHistoryModel
    {
        public ApprenticeEmploymentCheckMessageHistoryModel() { }

        public ApprenticeEmploymentCheckMessageHistoryModel(
            // Message history properties
            Guid messageHistoryId,
            DateTime messageHistorycreatedDateTime,
            // Employment check message properties
            Guid messageId,
            DateTime messageCreatedDateTime,
            long employmentCheckId,
            long uln,
            string nationalInsuranceNumber,
            string payeScheme,
            DateTime startDateTime,
            DateTime endDateTime,
            DateTime? employmentCheckedDateTime,
            bool? isEmployed,
            string returnCode,
            string returnMessage)
        {
            MessageHistoryId = new Guid();
            MessageHistoryCreatedDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            MessageId = messageId;
            MessageCreatedDateTime = messageCreatedDateTime;
            EmploymentCheckId = employmentCheckId;
            Uln = uln;
            NationalInsuranceNumber = nationalInsuranceNumber;
            PayeScheme = payeScheme;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            EmploymentCheckedDateTime = employmentCheckedDateTime;
            IsEmployed = isEmployed;
            ReturnCode = returnCode;
            ReturnMessage = returnMessage;
        }

        public ApprenticeEmploymentCheckMessageHistoryModel(ApprenticeEmploymentCheckMessageModel ApprenticeEmploymentCheckMessageModel)
        {
            MessageHistoryId = new Guid();
            MessageHistoryCreatedDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            MessageId = ApprenticeEmploymentCheckMessageModel.MessageId;
            MessageCreatedDateTime = ApprenticeEmploymentCheckMessageModel.MessageCreatedDateTime;
            EmploymentCheckId = ApprenticeEmploymentCheckMessageModel.EmploymentCheckId;
            Uln = ApprenticeEmploymentCheckMessageModel.Uln;
            NationalInsuranceNumber = ApprenticeEmploymentCheckMessageModel.NationalInsuranceNumber;
            PayeScheme = ApprenticeEmploymentCheckMessageModel.PayeScheme;
            StartDateTime = ApprenticeEmploymentCheckMessageModel.StartDateTime;
            EndDateTime = ApprenticeEmploymentCheckMessageModel.EndDateTime;
            EmploymentCheckedDateTime = ApprenticeEmploymentCheckMessageModel.EmploymentCheckedDateTime;
            IsEmployed = ApprenticeEmploymentCheckMessageModel.IsEmployed;
            ReturnCode = ApprenticeEmploymentCheckMessageModel.ReturnCode;
            ReturnMessage = ApprenticeEmploymentCheckMessageModel.ReturnMessage;
        }

        // Message history properties
        public Guid MessageHistoryId { get; set; }

        public DateTime MessageHistoryCreatedDateTime { get; set; }

        // Employment check message properties
        public Guid MessageId { get; set; }

        public DateTime MessageCreatedDateTime { get; set; }

        public long EmploymentCheckId { get; set; }

        public long Uln { get; set; }

        public string NationalInsuranceNumber { get; set; }

        public string PayeScheme { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public DateTime? EmploymentCheckedDateTime { get; set; }

        public bool? IsEmployed { get; set; }

        public string ReturnCode { get; set; }

        public string ReturnMessage { get; set; }

    }
}

