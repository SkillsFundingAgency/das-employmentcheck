using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SFA.DAS.EmploymentCheck.Domain.Entities
{
    public class EmploymentCheckRequest
    {
        public byte CheckType { get; set; }

        public Guid? CorrelationId { get; set; }

        public string Nino { get; set; }

        public string PayeScheme { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public bool? Employed { get; set; }

        public short? RequestCompletionStatus { get; set; }

        public ICollection<NinoApiResponse> NiNumberApiResponses { get; set; } = new Collection<NinoApiResponse>();

        public ICollection<PayeSchemeApiResponse> PayeSchemeApiResponses { get; set; } = new Collection<PayeSchemeApiResponse>();
    }
}
