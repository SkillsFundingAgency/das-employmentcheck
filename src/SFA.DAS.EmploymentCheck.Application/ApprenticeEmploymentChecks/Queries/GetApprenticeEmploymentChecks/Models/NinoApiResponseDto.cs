using SFA.DAS.EmploymentCheck.Application.Common.Mappings;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Application.ApprenticeEmploymentChecks.Queries.GetApprenticeEmploymentChecks.Models
{
    [Table("NiNumberApiResponse", Schema = "Cache")]

    public class NinoApiResponseDto
        : IMapFrom<NinoApiResponse>
    {
        public NinoApiResponseDto() {}

        //public NinoApiResponseDto(
        //    long? apprenticeEmploymentCheckId,
        //    Guid? correlationId,
        //    long uln,
        //    string niNumber,
        //    string httpResponse,
        //    short httpStatusCode,
        //    DateTime createdOn,
        //    DateTime lastUpdatedOn)
        //{
        //    ApprenticeEmploymentCheckId = apprenticeEmploymentCheckId;
        //    CorrelationId = correlationId;
        //    Uln = uln;
        //    NiNumber = niNumber;
        //    HttpResponse = httpResponse;
        //    HttpStatusCode = httpStatusCode;
        //    CreatedOn = createdOn;
        //    LastUpdatedOn = lastUpdatedOn;
        //}

        public long? ApprenticeEmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public long Uln { get; set; }

        public string NiNumber { get; set; }
    }
}
