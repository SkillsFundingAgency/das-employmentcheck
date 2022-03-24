namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class ApplicationSettings
    {
        public string DbConnectionString { get; set; }
        public string AllowedHashstringCharacters { get; set; }
        public string Hashstring { get; set; }
    }
}
