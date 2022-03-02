using System.Text.Json.Serialization;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class LearnerNiNumber
    {
        public LearnerNiNumber() { }

        public LearnerNiNumber(
            long uln,
            string niNumber)
        {
            Uln = uln;
            NiNumber = niNumber;
        }

        [JsonPropertyName("uln")]
        public long Uln { get; set; }

        [JsonPropertyName("niNumber")]
        public string NiNumber { get; set; }
    }
}
