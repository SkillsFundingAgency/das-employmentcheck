using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Api.Services;

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

        public async Task<RegisterCheckResult> Handle(RegisterCheckCommand command,
            CancellationToken cancellationToken)
        {
            //Validate the command

            var validationResult = _commandValidator.Validate(command);

            if (!string.IsNullOrEmpty(validationResult.ErrorType) && !string.IsNullOrEmpty(validationResult.ErrorMessage))
            {
                return validationResult;
            }

            //Get VersionId if row is present and increment it if it is
            
            var existingEmploymentCheck = await _employmentCheckService.CheckForExistingEmploymentCheck(command.CorrelationId);

            validationResult.VersionId = existingEmploymentCheck != null ? (existingEmploymentCheck.VersionId += 1): 1;
            
            //Insert new EmploymentCheck into the db

            var employmentCheck = CreateNewEmploymentCheck(command, validationResult.VersionId);

            _employmentCheckService.InsertEmploymentCheck(employmentCheck);

            //Return the VersionId
            return validationResult;
        }

        private Functions.Application.Models.EmploymentCheck CreateNewEmploymentCheck(RegisterCheckCommand command, int? versionId)
        {
            var lastId = _employmentCheckService.GetLastId();

            return new Functions.Application.Models.EmploymentCheck
            {
                AccountId = command.ApprenticeshipAccountId,
                ApprenticeshipId = command.ApprenticeshipId,
                CheckType = command.CheckType,
                CorrelationId = command.CorrelationId,
                CreatedOn = DateTime.Now,
                Employed = null,
                Id = lastId + 1,
                LastUpdatedOn = DateTime.Now,
                MinDate = command.MinDate,
                MaxDate = command.MaxDate,
                RequestCompletionStatus = null,
                VersionId = (short)versionId
            };
        }
    }
}