namespace SFA.DAS.EmploymentCheck.Api.Commands.RegisterCheckCommand
{
    public interface IRegisterCheckCommandValidator
    {
        public RegisterCheckResult Validate(RegisterCheckCommand command);
    }
}