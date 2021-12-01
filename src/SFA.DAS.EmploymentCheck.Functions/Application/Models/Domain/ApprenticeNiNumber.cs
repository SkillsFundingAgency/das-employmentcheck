using System.Text.Json.Serialization;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class ApprenticeNiNumber
    {
        public ApprenticeNiNumber() { }

        public ApprenticeNiNumber(
            long uln,
            string nationalInsuranceNumber)
        {
            Uln = uln;
            NationalInsuranceNumber = nationalInsuranceNumber;
        }

        [JsonPropertyName("uln")]
        public long ULN { get; set; }

        [JsonPropertyName("niNumber")]
        public string NationalInsuranceNumber { get; set; }
    }
}
