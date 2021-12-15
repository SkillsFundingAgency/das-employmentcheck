using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto
{
    [Table("Cache.EmploymentCheckMessage")]

    public class EmploymentCheckMessage
    {
        public EmploymentCheckMessage() { }

        public EmploymentCheckMessage(
            ILogger<EmploymentCheckMessage> logger,
            long id,
            long employmentCheckId,
            Guid correlationId,
            long uln,
            string nationalInsuranceNumber,
            string payeScheme,
            DateTime minDateTime,
            DateTime maxDateTime,
            bool? employed,
            DateTime? lastEmploymentCheck,
            short responseHttpStatusCode,
            string responseMessage,
            DateTime lastUpdated,
            DateTime createdOn)
        {
            try
            {
                Id = id;
                EmploymentCheckId = employmentCheckId;
                CorrelationId = correlationId;
                Uln = uln;
                NationalInsuranceNumber = nationalInsuranceNumber;
                PayeScheme = payeScheme;
                MinDateTime = minDateTime;
                MaxDateTime = maxDateTime;
                Employed = employed;
                LastEmploymentCheck = lastEmploymentCheck;
                ResponseHttpStatusCode = responseHttpStatusCode;
                ResponseMessage = responseMessage;
                LastUpdated = lastUpdated;
                CreatedOn = createdOn;
            }
            catch (Exception ex)
            {
                logger.LogError($"\n\nEmploymentCheckMessage() Constructor: Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        public long Id { get; set; }

        public long EmploymentCheckId { get; set; }

        public Guid CorrelationId { get; set; }

        public long Uln { get; set; }

        public string NationalInsuranceNumber { get; set; }

        public string PayeScheme { get; set; }

        public DateTime MinDateTime { get; set; }

        public DateTime MaxDateTime { get; set; }

        public bool? Employed { get; set; }

        public DateTime? LastEmploymentCheck { get; set; }

        public short ResponseHttpStatusCode { get; set; }

        public string ResponseMessage { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}

