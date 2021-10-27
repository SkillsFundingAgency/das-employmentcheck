namespace SFA.DAS.EmploymentCheck.Functions.Helpers
{
    public interface ILoggerAdapter<T>
    {
        void LogInformation(string message);
    }

    public interface ILoggerAdapter
    {
        void LogInformation(string message);
    }
}