using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class ApprenticeEmploymentCheckMessageModel
    {
        public ApprenticeEmploymentCheckMessageModel() { }

        public ApprenticeEmploymentCheckMessageModel(
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

