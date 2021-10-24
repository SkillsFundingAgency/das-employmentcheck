namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class ApprenticeNiNumber
    {
        public ApprenticeNiNumber() { }

        public ApprenticeNiNumber(
            long uln,
            string nationalInsuranceNumber)
        {
            ULN = uln;
            NationalInsuranceNumber = nationalInsuranceNumber;
        }

        public long ULN { get; set; }

        public string NationalInsuranceNumber { get; set; }
    }
}
