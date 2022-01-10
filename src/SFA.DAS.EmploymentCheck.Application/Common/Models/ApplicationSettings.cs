namespace SFA.DAS.EmploymentCheck.Application.Common.Models
{
    public class ApplicationSettings
    {
        public string DbConnectionString { get; set; }

        public int BatchSize { get; set; }

        public string AllowedHashstringCharacters { get; set; }

        public string Hashstring { get; set; }
    }
}
