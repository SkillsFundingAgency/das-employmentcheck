using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateEmploymentCheckCacheRequestsOrchestrator
    {
        private const string ThisClassName = nameof(CreateEmploymentCheckCacheRequestsOrchestrator);
        private const string NinoNotFound = "NinoNotFound";
        private const string NinoInvalid = "NinoInvalid";
        private const string NinoFailure = "NinoFailure";
        private const string PayeNotFound = "PAYENotFound";
        private const string PayeFailure = "PAYEFailure";

        private readonly ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> _logger;

        public CreateEmploymentCheckCacheRequestsOrchestrator(
            ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(CreateEmploymentCheckCacheRequestsOrchestrator))]
        public async Task CreateEmploymentCheckRequestsTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.CreateEmploymentCheckRequestsTask";

            try
            {
                var employmentCheck = await context.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetEmploymentCheckActivity), null);
                if (employmentCheck != null)
                {
                    var learnerNiNumberTask = context.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumberActivity), employmentCheck);
                    var employerPayeSchemesTask = context.CallActivityAsync<EmployerPayeSchemes>(nameof(GetEmployerPayeSchemesActivity), employmentCheck);

                    await Task.WhenAll(learnerNiNumberTask, employerPayeSchemesTask);

                    var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumberTask.Result, employerPayeSchemesTask.Result);
                    if (IsValidEmploymentCheckData(employmentCheckData))
                    {
                        await context.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), employmentCheckData);
                    }
                    else
                    {
                        await context.CallActivityAsync(nameof(StoreCompletedEmploymentCheckActivity), employmentCheck);
                    }
                }
                else
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: {nameof(GetEmploymentCheckActivity)} returned no results. Nothing to process.");

                    // No data found so sleep for 10 seconds then execute the orchestrator again
                    var sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{thisMethodName}: Exception caught - {e.Message}. {e.StackTrace}");
            }
            finally
            {
                context.ContinueAsNew(null);
            }
        }

        public bool IsValidEmploymentCheckData(EmploymentCheckData employmentCheckData)
        {
            var isValidEmploymentcheckData = true;

            var isValidNino = IsValidNino(employmentCheckData);
            var isValidPayeScheme = IsValidPayeScheme(employmentCheckData);

            if (!isValidNino || !isValidPayeScheme)
            {
                isValidEmploymentcheckData = false;
            }

            return isValidEmploymentcheckData;
        }

        public bool IsValidNino(EmploymentCheckData employmentCheckData)
        {
            const int validNinoLength = 9;

            if (employmentCheckData.ApprenticeNiNumber == null)
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoFailure;
                return false;
            }

            if (string.IsNullOrEmpty(employmentCheckData.ApprenticeNiNumber.NiNumber))
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoNotFound;
                return false;
            }

            if (employmentCheckData.ApprenticeNiNumber.HttpStatusCode == HttpStatusCode.NoContent)
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoNotFound;
                return false;
            }

            if (employmentCheckData.ApprenticeNiNumber.HttpStatusCode == HttpStatusCode.NotFound)
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoNotFound;
                return false;
            }

            if ((int)employmentCheckData.ApprenticeNiNumber.HttpStatusCode >= 400
                && (int)employmentCheckData.ApprenticeNiNumber.HttpStatusCode <= 599)
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoFailure;
                return false;
            }

            if (employmentCheckData.ApprenticeNiNumber.NiNumber.Length < validNinoLength)
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoInvalid;
                return false;
            }

            return true;
        }

        public bool IsValidPayeScheme(EmploymentCheckData employmentCheckData)
        {
            if (!IsValidPayeSchemeNullOrEmptyChecks(employmentCheckData))
            {
                return false;
            }

            if (employmentCheckData.EmployerPayeSchemes.HttpStatusCode == HttpStatusCode.NoContent)
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeNotFound : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeNotFound}";
                return false;
            }

            if (employmentCheckData.EmployerPayeSchemes.HttpStatusCode == HttpStatusCode.NotFound)
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeNotFound : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeNotFound}";
                return false;
            }

            if ((int)employmentCheckData.EmployerPayeSchemes.HttpStatusCode >= 400
                && (int)employmentCheckData.EmployerPayeSchemes.HttpStatusCode <= 599)
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeFailure : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeFailure}";
                return false;
            }

            return true;
        }

        public bool IsValidPayeSchemeNullOrEmptyChecks(EmploymentCheckData employmentCheckData)
        {
            if (employmentCheckData.EmployerPayeSchemes == null)
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeFailure : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeFailure}";
                return false;
            }

            if (!employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeNotFound : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeNotFound}";
                return false;
            }

            if (employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
            {
                var emptyValue = false;
                foreach (var employerPayeScheme in employmentCheckData.EmployerPayeSchemes.PayeSchemes.Where(x => x.Length == 0))
                {
                    emptyValue = true;  // there is no longer any validation in the 'create cache request' code so this is to stop a request being created with a 'blank' paye scheme
                }
                if (emptyValue == true)
                {
                    employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeNotFound : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeNotFound}";
                    return false;
                }
            }

            return true;
        }
    }
}