using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Api.Application.Services;

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

        private void Save(RegisterCheckCommand command, RegisterCheckResult validationResult)
        {
            var employmentCheck = CreateNewEmploymentCheck(command, validationResult.VersionId);

            _employmentCheckService.InsertEmploymentCheck(employmentCheck);
        }

        private async Task GetNextVersionId(RegisterCheckCommand command, RegisterCheckResult validationResult)
        {
            var existingEmploymentCheck = await _employmentCheckService.CheckForExistingEmploymentCheck(command.CorrelationId);

            validationResult.VersionId = (short) (existingEmploymentCheck?.VersionId + 1 ?? 1);
        }

        private Application.Models.EmploymentCheck CreateNewEmploymentCheck(RegisterCheckCommand command, short? versionId)
        {
            return new Application.Models.EmploymentCheck
            {
                AccountId = command.ApprenticeshipAccountId,
                ApprenticeshipId = command.ApprenticeshipId,
                CheckType = command.CheckType,
                Uln = command.Uln,
                CorrelationId = command.CorrelationId,
                CreatedOn = DateTime.Now,
                Employed = null,
                LastUpdatedOn = DateTime.Now,
                MinDate = command.MinDate,
                MaxDate = command.MaxDate,
                RequestCompletionStatus = null,
                VersionId = versionId ?? 1
            };
        }
    }
}