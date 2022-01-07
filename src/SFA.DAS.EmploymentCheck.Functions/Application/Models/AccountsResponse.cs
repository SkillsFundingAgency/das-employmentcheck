using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    public class AccountsResponse
    {
        public AccountsResponse () { }

        public AccountsResponse(
            long? apprenticeEmploymentCheckId,
            Guid? correlationId,
            long accountId,
            string payeSchemes,
            string httpResponse,
            short httpStatusCode)
        {
            ApprenticeEmploymentCheckId = apprenticeEmploymentCheckId;
            CorrelationId = correlationId;
            AccountId = accountId;
            PayeSchemes = payeSchemes;
            HttpResponse = httpResponse;
            HttpStatusCode = httpStatusCode;
        }

        public long? ApprenticeEmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public long AccountId { get; set; }

        public string PayeSchemes { get; set; }

        public string HttpResponse { get; set; }

        public short HttpStatusCode { get; set; }
    }
}
