namespace SFA.DAS.EmploymentCheck.Commands.RegisterCheck
{
    public interface IRegisterCheckCommandValidator
    {
        public RegisterCheckResult Validate(RegisterCheckCommand command);
    }
}