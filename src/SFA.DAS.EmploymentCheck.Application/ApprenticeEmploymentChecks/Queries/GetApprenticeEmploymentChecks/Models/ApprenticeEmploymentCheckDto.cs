using SFA.DAS.EmploymentCheck.Domain.Common;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Application.ApprenticeEmploymentChecks.Queries.GetApprenticeEmploymentChecks.Models
{
    public class ApprenticeEmploymentCheckDto
        : AuditableEntity
    {
        public ApprenticeEmploymentCheckDto() { }

        //public ApprenticeEmploymentCheckDto(
        //    long id,
        //    Guid? correlationId,
        //    string checkType,
        //    bool checkAllPayeSchemes,
        //    long uln,
        //    long? apprenticeshipId,
        //    long accountId,
        //    DateTime minDate,
        //    DateTime maxDate,
        //    bool? employed,
        //    IList<NinoApiResponse> ninoApiResponses,
        //    IList<PayeSchemeApiResponse> payeSchemeApiResponses)
        //{
        //    Id = id;
        //    CorrelationId = correlationId;
        //    CheckType = checkType;
        //    CheckAllPayeSchemes = checkAllPayeSchemes;
        //    Uln = uln;
        //    ApprenticeshipId = apprenticeshipId;
        //    AccountId = accountId;
        //    MinDate = minDate;
        //    MaxDate = maxDate;
        //    Employed = employed;
        //    NinoApiResponses = ninoApiResponses;
        //    PayeSchemeApiResponses = payeSchemeApiResponses;
        //}

        public Guid? CorrelationId { get; set; }

        public string CheckType { get; set; }

        public bool CheckAllPayeSchemes { get; set; }

        public long Uln { get; set; }

        public long? ApprenticeshipId { get; set; }

        public long AccountId { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public bool? Employed { get; set; }

        public virtual IList<NinoApiResponseDto> NinoApiResponses { get; set; } = new List<NinoApiResponseDto>();

        public virtual IList<PayeSchemeApiResponseDto> PayeSchemeApiResponses { get; set; } = new List<PayeSchemeApiResponseDto>();
    }
}

