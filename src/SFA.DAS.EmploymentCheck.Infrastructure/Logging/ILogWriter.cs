namespace SFA.DAS.EmploymentCheck.Infrastructure.Logging
{
    public interface ILogWriter
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public Log Log { get; }
    }
}
