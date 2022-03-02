using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Commands.RegisterCheck
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

            Save(command);

            return result;
        }

        private void Save(RegisterCheckCommand command )
        {
            var employmentCheck = CreateNewEmploymentCheck(command);

            _employmentCheckService.InsertEmploymentCheck(employmentCheck);
        }

        private static Data.Models.EmploymentCheck CreateNewEmploymentCheck(RegisterCheckCommand command)
        {
            return new Data.Models.EmploymentCheck(
                command.CorrelationId,
                command.CheckType,
                command.Uln,
                command.ApprenticeshipId,
                command.ApprenticeshipAccountId,
                command.MinDate,
                command.MaxDate
            );
        }
    }
}