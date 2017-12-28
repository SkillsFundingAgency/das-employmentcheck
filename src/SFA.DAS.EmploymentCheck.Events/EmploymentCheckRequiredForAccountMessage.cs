using System;
using System.Collections.Generic;
using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.EmploymentCheck.Events
{
    [MessageGroup("employment_check_for_account")]
    public class EmploymentCheckRequiredForAccountMessage
    {
        public EmploymentCheckRequiredForAccountMessage() { }

        public EmploymentCheckRequiredForAccountMessage(string nationalInsuranceNumber, long uln, long employerAccountId, long ukprn, DateTime actualStartDate, IEnumerable<string> payeSchemes)
        {
            NationalInsuranceNumber = nationalInsuranceNumber;
            Uln = uln;
            EmployerAccountId = employerAccountId;
            Ukprn = ukprn;
            ActualStartDate = actualStartDate;
            PayeSchemes = payeSchemes;
        }

        public string NationalInsuranceNumber { get; set; }
        public long Uln { get; set; }
        public long EmployerAccountId { get; set; }
        public long Ukprn { get; set;  }
        public DateTime ActualStartDate { get; set; }
        public IEnumerable<string> PayeSchemes { get; set; }
    }
}
