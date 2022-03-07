using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    [Table("Cache.AccountsResponse")]
    public class AccountsResponse
    {
        public AccountsResponse() { }

        public AccountsResponse(
            long id,
            long? apprenticeEmploymentCheckId,
            Guid? correlationId,
            long accountId,
            string payeSchemes,
            string httpResponse,
            short httpStatusCode,
            DateTime? lastUpdatedOn
        )
        {
            Id = id;
            ApprenticeEmploymentCheckId = apprenticeEmploymentCheckId;
            CorrelationId = correlationId;
            AccountId = accountId;
            PayeSchemes = payeSchemes;
            HttpResponse = httpResponse;
            HttpStatusCode = httpStatusCode;
            CreatedOn = DateTime.Now;
            LastUpdatedOn = lastUpdatedOn;
        }

        [Key]
        public long Id { get; set; }

        public long? ApprenticeEmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public long AccountId { get; set; }

        public string PayeSchemes { get; set; }

        public string HttpResponse { get; set; }

        public short HttpStatusCode { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public static AccountsResponse CreateResponse(long employmentCheckId, Guid correlationId, long accountId, string httpResponse, short statusCode)
        {
            return new AccountsResponse
            {
                ApprenticeEmploymentCheckId = employmentCheckId,
                CorrelationId = correlationId,
                AccountId = accountId,
                HttpResponse = $"{httpResponse[Range.EndAt(Math.Min(8000, httpResponse.Length))]}",
                HttpStatusCode = statusCode,
                LastUpdatedOn = DateTime.Now
            };
        }

        public static AccountsResponse CreateErrorResponse(long employmentCheckId, Guid correlationId, long accountId)
        {
            return new AccountsResponse
            {
                ApprenticeEmploymentCheckId = employmentCheckId,
                CorrelationId = correlationId,
                AccountId = accountId,
                HttpStatusCode = (short)System.Net.HttpStatusCode.InternalServerError,
                LastUpdatedOn = DateTime.Now
            };
        }

        public void SetPayeSchemes(IList<string> payeSchemes)
        {
           PayeSchemes = string.Join(',', payeSchemes);
        }
    }
}
