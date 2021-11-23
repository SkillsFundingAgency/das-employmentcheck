namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class DcApiSettings
    {
        public string BaseUrl { get; set; }
        public string LearnerApi { get; set; }
        public string LearnerNiAPi { get; set; }
        public int PageSize { get; set; }
        public int Ukprn { get; set; }
        public int TaskSize { get; set; }
    }
}