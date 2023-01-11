namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class EmployerAccountApiConfiguration : IApiConfiguration
    {
        public string Url { get; set; }
        public string Identifier { get; set; }
        public string BaseUrl { get => Url; set =>  Url = value; } 
    }
}
