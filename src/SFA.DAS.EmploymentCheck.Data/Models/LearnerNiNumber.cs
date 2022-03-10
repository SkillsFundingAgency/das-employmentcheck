using System.Net;
using System.Text.Json.Serialization;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class LearnerNiNumber
    {
        public LearnerNiNumber(
            long uln,
            string niNumber,
            HttpStatusCode httpStatusCode
        )
        {
            Uln = uln;
            NiNumber = niNumber;
            HttpStatusCode = httpStatusCode;
        }

        [JsonPropertyName("uln")]
        public long Uln { get; set; }

        [JsonPropertyName("niNumber")]
        public string NiNumber { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
