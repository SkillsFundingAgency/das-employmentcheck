using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Application.Interfaces.EmployerAccount;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetPayeSchemes
{
    public class GetPayeSchemesQueryHandler
        : IRequestHandler<GetPayeSchemesQueryRequest,
            GetPayeSchemesQueryResult>
    {
        #region Private members
        private ILogger<GetPayeSchemesQueryHandler> _logger;
        private readonly IEmployerAccountClient _employerAccountClient;
        #endregion Private members

        #region Constructors
        public GetPayeSchemesQueryHandler(
            ILogger<GetPayeSchemesQueryHandler> logger,
            IEmployerAccountClient employerAccountClient)
        {
            _logger = logger;
            _employerAccountClient = employerAccountClient;
        }
        #endregion Constructors

        #region Handle
        public async Task<GetPayeSchemesQueryResult> Handle(
            GetPayeSchemesQueryRequest getPayeSchemesRequest,
            CancellationToken cancellationToken)
        {
            const string thisMethodName = "GetPayeSchemesQueryHandler.Handle";

            Guard.Against.Null(getPayeSchemesRequest, nameof(getPayeSchemesRequest));
            Guard.Against.Null(getPayeSchemesRequest.EmploymentCheckBatch, nameof(getPayeSchemesRequest.EmploymentCheckBatch));

            IList<EmployerPayeSchemes> employersPayeSchemes = new List<EmployerPayeSchemes>();
            try
            {
                // Call the application client to get the employer PAYE schemes for the apprentices
                employersPayeSchemes = await _employerAccountClient.GetEmployersPayeSchemes(getPayeSchemesRequest.EmploymentCheckBatch);

                if (employersPayeSchemes.Count > 0)
                {
                    _logger.LogInformation($"{thisMethodName} returned {employersPayeSchemes.Count} PAYE scheme(s)");
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName} returned null/zero PAYE schemes");
                    employersPayeSchemes = new List<EmployerPayeSchemes>(); // return empty list rather than null
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new GetPayeSchemesQueryResult(employersPayeSchemes);
        }
        #endregion Handle
    }
}