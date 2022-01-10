using System;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmploymentCheck.Domain.Common;

namespace SFA.DAS.EmploymentCheck.Domain.Entities
{
    [Table("Cache.AccountsResponse")]

    public class AccountsResponse
        : Entity
    {
        public AccountsResponse () { }

        public AccountsResponse(
            long? employmentCheckId,
            Guid? correlationId,
            long accountId,
            string payeSchemes,
            string httpResponse,
            short httpStatusCode)
        {
            EmploymentCheckId = employmentCheckId;
            CorrelationId = correlationId;
            AccountId = accountId;
            PayeSchemes = payeSchemes;
            HttpResponse = httpResponse;
            HttpStatusCode = httpStatusCode;
        }

        public long? EmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public long AccountId { get; set; }

        public string PayeSchemes { get; set; }

        public string HttpResponse { get; set; }

        public short HttpStatusCode { get; set; }
    }
}
