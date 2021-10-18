namespace SFA.DAS.EmploymentCheck.Functions.Dtos
{
    public class LearnerNinoDto
    {
        public LearnerNinoDto() { }

        public LearnerNinoDto(
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
