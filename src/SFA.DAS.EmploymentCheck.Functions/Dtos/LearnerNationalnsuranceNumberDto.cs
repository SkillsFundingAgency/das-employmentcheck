namespace SFA.DAS.EmploymentCheck.Functions.Dtos
{
    public class LearnerNationalnsuranceNumberDto
    {
        public LearnerNationalnsuranceNumberDto() { }

        public LearnerNationalnsuranceNumberDto(
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
