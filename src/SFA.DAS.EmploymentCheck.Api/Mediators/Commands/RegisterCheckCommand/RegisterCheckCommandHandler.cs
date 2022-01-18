using MediatR;
using SFA.DAS.EmploymentCheck.Api.Application.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand
{
    public class RegisterCheckCommandHandler : IRequestHandler<RegisterCheckCommand, RegisterCheckResult>
    {
        private readonly IEmploymentCheckService _employmentCheckService;
        private readonly IRegisterCheckCommandValidator _commandValidator;

        public RegisterCheckCommandHandler(IEmploymentCheckService employmentCheckService, IRegisterCheckCommandValidator commandValidator)
        {
            _employmentCheckService = employmentCheckService;
            _commandValidator = commandValidator;
        }

        public async Task<RegisterCheckResult> Handle(RegisterCheckCommand command, CancellationToken cancellationToken)
        {
            var result = _commandValidator.Validate(command);

            if (result.Invalid()) return result;

            await GetNextVersionId(command, result);

            Save(command, result);

            return result;
        }

        private void Save(RegisterCheckCommand command, RegisterCheckResult result)
        {
            var employmentCheck = CreateNewEmploymentCheck(command, result.VersionId);

            _employmentCheckService.InsertEmploymentCheck(employmentCheck);
        }

        private async Task GetNextVersionId(RegisterCheckCommand command, RegisterCheckResult result)
        {
            var existingEmploymentCheck = await _employmentCheckService.GetLastEmploymentCheck(command.CorrelationId);

            result.VersionId = (short) (existingEmploymentCheck?.VersionId + 1 ?? 1);
        }

        private static Application.Models.EmploymentCheck CreateNewEmploymentCheck(RegisterCheckCommand command,
            short? versionId)
        {
            return new Application.Models.EmploymentCheck(
                command.CorrelationId,
                command.CheckType,
                command.Uln,
                command.ApprenticeshipId,
                command.ApprenticeshipAccountId,
                command.MinDate,
                command.MaxDate,
                versionId ?? 1
            );
        }
    }
}