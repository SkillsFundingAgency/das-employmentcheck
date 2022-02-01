using Dapper.Contrib.Extensions;
using System.Text.Json.Serialization;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
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

        [Key]
        [JsonPropertyName("uln")]
        public long Uln { get; set; }

        [JsonPropertyName("niNumber")]
        public string NiNumber { get; set; }
    }
}
