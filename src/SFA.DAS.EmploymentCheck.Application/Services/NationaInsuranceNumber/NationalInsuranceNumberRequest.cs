namespace SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber
{
    public struct NationalInsuranceNumberRequest
    {
        public Data.Models.EmploymentCheck EmploymentCheck { get; private set; }
        public string AcademicYear { get; private set; }

        public NationalInsuranceNumberRequest(Data.Models.EmploymentCheck employmentCheck, string academicYear = null)
        {
            EmploymentCheck = employmentCheck;
            AcademicYear = academicYear;
        }
    }
}
