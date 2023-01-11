namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class DataCollectionsApiConfiguration : IApiConfiguration
    {
        public string BaseUrl { get; set; }
        public string Tenant { get; set; }
        public string IdentifierUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Path { get; set; }
        public string AcademicYearsPath { get; set; }
        public int NumberOfAcademicYearsToSearch { get; set; }
        public int AcademicYearsCacheDurationSecs { get; set; }

        public DataCollectionsApiConfiguration()
        {
            Path = @"/api/v1/ilr-data/learnersNi";
            AcademicYearsPath = @"/api/v1/academic-years";
            NumberOfAcademicYearsToSearch = 2;
            AcademicYearsCacheDurationSecs = 3600;
        }
    }
}