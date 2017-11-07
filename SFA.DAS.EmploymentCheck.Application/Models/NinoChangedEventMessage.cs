using System;

namespace SFA.DAS.EmploymentCheck.Application.Models
{
    public class NinoChangedEventMessage
    {
        public string NiNumber { get; set; }
        public string Uln { get; set; }
        public long EmployerReferenceNumber { get; set; }
        public long ApprenticeShipId { get; set; }
        public string UKPRN { get; set; }
        public DateTime ActualStartDate { get; set; }
        public long Id { get; set; }
    }
}
