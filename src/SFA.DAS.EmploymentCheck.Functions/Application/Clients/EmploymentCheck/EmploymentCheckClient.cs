using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public class EmploymentCheckClient
        : IEmploymentCheckClient
    {
        private EmploymentCheckDbConfiguration _employmentCheckDbConfiguration;
        private IEmploymentCheckService _employmentCheckService;
        private ILoggerAdapter<IEmploymentCheckClient> _logger;

        public EmploymentCheckClient(
            EmploymentCheckDbConfiguration employmentCheckDbConfiguration,
            IEmploymentCheckService employmentCheckService,
            ILoggerAdapter<IEmploymentCheckClient> logger)
        {
            _employmentCheckDbConfiguration = employmentCheckDbConfiguration;
            _employmentCheckService = employmentCheckService;
            _logger = logger;
        }

        public async Task<IList<Apprentice>> GetApprentices()
        {
            var thisMethodName = "EmploymentCheckClient.GetApprentices()";

            IList<Apprentice> apprentices;
            try
            {
                /* TODO: Remove assingment below and setup config for local and azure setting for connection string */
                _employmentCheckDbConfiguration.ConnectionString = "Anything for local stub testing";
                if (_employmentCheckDbConfiguration.ConnectionString != null)
                {
                    var connectionString = _employmentCheckDbConfiguration.ConnectionString;
                    apprentices = await _employmentCheckService.GetApprentices();

                    if(apprentices != null && apprentices.Count > 0)
                    {
                        _logger.LogInformation($"{thisMethodName}: returned [{apprentices.Count}] apprentices");
                        //Log.WriteLog(_logger, thisMethodName, $"returned [{apprentices.Count}] apprentices.");
                    }
                    else
                    {
                        _logger.LogInformation($"{thisMethodName}: returned null/zero apprentices");
                        //Log.WriteLog(_logger, thisMethodName, "returned null/zero apprentices.");
                        apprentices = new List<Apprentice>();
                    }
                }
                else
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: Employment Check Db Connection String NOT Configured");
                    apprentices = new List<Apprentice>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                apprentices = new List<Apprentice>();
            }

            return apprentices;
        }
    }
}
