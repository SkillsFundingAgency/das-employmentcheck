namespace SFA.DAS.EmploymentCheck.TokenServiceStub.Services
{
    public interface ITotpService
    {
        string Generate(string secret);
    }
}