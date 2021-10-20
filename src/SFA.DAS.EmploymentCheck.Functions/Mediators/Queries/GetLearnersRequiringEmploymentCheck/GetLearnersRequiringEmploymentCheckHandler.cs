using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.DataAccess;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetLearnersRequiringEmploymentCheck
{
    public class GetLearnersRequiringEmploymentCheckHandler : IRequestHandler<GetLearnersRequiringEmploymentCheckRequest, GetLearnersRequiringEmploymentCheckResult>
    {
        private IEmploymentChecksRepository _repository;
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private const string AzureResource = "https://database.windows.net/";
        private ILogger<GetLearnersRequiringEmploymentCheckHandler> _logger;

        public GetLearnersRequiringEmploymentCheckHandler(
            IEmploymentChecksRepository repository,
            ILogger<GetLearnersRequiringEmploymentCheckHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetLearnersRequiringEmploymentCheckResult> Handle(
            GetLearnersRequiringEmploymentCheckRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = "\n\n GetLearnersRequiringEmploymentCheckHandler.Handle();";

            List<LearnerRequiringEmploymentCheckDto> learnersRequiringEmploymentCheckDtos = null;

            try
            {
                var sqlConnection = await CreateConnection();

                // Call the data repository to get the learners requiring and employment check
                learnersRequiringEmploymentCheckDtos = await _repository.GetLearnersRequiringEmploymentChecks(sqlConnection);

                if (learnersRequiringEmploymentCheckDtos == null)
                {
                    learnersRequiringEmploymentCheckDtos = new List<LearnerRequiringEmploymentCheckDto>(); // return empty list rather than null
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(new GetLearnersRequiringEmploymentCheckResult(learnersRequiringEmploymentCheckDtos));
        }

        private async Task<SqlConnection> CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            if (_azureServiceTokenProvider != null)
            {
                connection.AccessToken = await _azureServiceTokenProvider.GetAccessTokenAsync(AzureResource);
            }

            return connection;
        }
    }
}
