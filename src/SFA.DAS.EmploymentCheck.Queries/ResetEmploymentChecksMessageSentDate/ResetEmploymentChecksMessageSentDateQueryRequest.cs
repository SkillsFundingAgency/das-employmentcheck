using System;
using System.Net.Http;

namespace SFA.DAS.EmploymentCheck.Queries.ResetEmploymentChecksMessageSentDate
{
    public class ResetEmploymentChecksMessageSentDateQueryRequest : IQuery
    {
        public ResetEmploymentChecksMessageSentDateQueryRequest(string employmentCheckMessageSentData)
        {
            EmploymentCheckMessageSentData = employmentCheckMessageSentData;
        }

        public string EmploymentCheckMessageSentData { get; set; }
    }
}
