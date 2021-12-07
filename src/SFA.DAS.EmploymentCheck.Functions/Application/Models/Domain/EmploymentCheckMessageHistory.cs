using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class EmploymentCheckMessageHistory
    {
        public EmploymentCheckMessageHistory() { }

        public EmploymentCheckMessageHistory(
            ILogger<EmploymentCheckMessageHistory> logger,
            long id,
            long messageId,
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
            DateTime messageCreatedOn,
            DateTime createdOn)
        {
            try
            {
                Id = id;
                Id = messageId;
                EmploymentCheckId = employmentCheckId;
                CorrelationId = correlationId;
                Uln = uln;
                NationalInsuranceNumber = nationalInsuranceNumber;
                PayeScheme = payeScheme;
                MinDateTime = minDateTime;
                MaxDateTime = maxDateTime;
                LastEmploymentCheck = lastEmploymentCheck;
                Employed = employed;
                ResponseHttpStatusCode = responseHttpStatusCode;
                ResponseMessage = responseMessage;
                MessageCreatedOn = messageCreatedOn;
                CreatedOn = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                logger.LogError($"\n\nEmploymentCheckMessageHistory() Constructor: Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }

        public EmploymentCheckMessageHistory(EmploymentCheckMessage employmentCheckMessageModel)
        {
            MessageId = employmentCheckMessageModel.Id;
            EmploymentCheckId = employmentCheckMessageModel.EmploymentCheckId;
            CorrelationId = employmentCheckMessageModel.CorrelationId;
            Uln = employmentCheckMessageModel.Uln;
            NationalInsuranceNumber = employmentCheckMessageModel.NationalInsuranceNumber;
            PayeScheme = employmentCheckMessageModel.PayeScheme;
            MinDateTime = employmentCheckMessageModel.MinDateTime;
            MaxDateTime = employmentCheckMessageModel.MaxDateTime;
            Employed = employmentCheckMessageModel.Employed;
            LastEmploymentCheck = employmentCheckMessageModel.LastEmploymentCheck;
            ResponseHttpStatusCode = employmentCheckMessageModel.ResponseHttpStatusCode;
            ResponseMessage = employmentCheckMessageModel.ResponseMessage;
            MessageCreatedOn = employmentCheckMessageModel.CreatedOn;
            CreatedOn = DateTime.UtcNow;
        }

        public long Id { get; set; }

        public long MessageId { get; set; }

        public Guid CorrelationId { get; set; }

        public long EmploymentCheckId { get; set; }

        public long Uln { get; set; }

        public string NationalInsuranceNumber { get; set; }

        public string PayeScheme { get; set; }

        public DateTime MinDateTime { get; set; }

        public DateTime MaxDateTime { get; set; }

        public bool? Employed { get; set; }

        public DateTime? LastEmploymentCheck { get; set; }

        public short ResponseHttpStatusCode { get; set; }

        public string ResponseMessage { get; set; }

        public DateTime MessageCreatedOn { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}

