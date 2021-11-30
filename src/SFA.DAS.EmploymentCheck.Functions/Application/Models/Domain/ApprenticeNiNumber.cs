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

        public long Uln { get; set; }

        public string NationalInsuranceNumber { get; set; }
    }
}
