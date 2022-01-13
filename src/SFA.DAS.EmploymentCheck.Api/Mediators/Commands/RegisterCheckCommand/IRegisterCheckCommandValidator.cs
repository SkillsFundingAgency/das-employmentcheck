namespace SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand
{
    public interface IRegisterCheckCommandValidator
    {
        public RegisterCheckResult Validate(RegisterCheckCommand command);
    }
}