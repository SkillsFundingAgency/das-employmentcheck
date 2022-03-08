using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateEmploymentCheckCacheRequestsOrchestrator
    {
        private const string ThisClassName = nameof(CreateEmploymentCheckCacheRequestsOrchestrator);
        private readonly ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> _logger;

        public CreateEmploymentCheckCacheRequestsOrchestrator(
            ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> logger
        )
        {
            _logger = logger;
        }

        [FunctionName(nameof(CreateEmploymentCheckCacheRequestsOrchestrator))]
        public async Task CreateEmploymentCheckRequestsTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.CreateEmploymentCheckRequestsTask";
            var checkActivityName = nameof(GetEmploymentCheckActivity);
            var ninoActivityName = nameof(GetLearnerNiNumberActivity);
            var payeActivityName = nameof(GetEmployerPayeSchemesActivity);
            var requestActivityName = nameof(CreateEmploymentCheckCacheRequestActivity);
            var storeActivityName = nameof(StoreCompletedEmploymentCheckActivity);

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Started.");

                var employmentCheck = await context.CallActivityAsync<Data.Models.EmploymentCheck>(checkActivityName, null);
                if (employmentCheck != null && employmentCheck.Id > 0)
                {
                    var learnerNiNumberTask = context.CallActivityAsync<LearnerNiNumber>(ninoActivityName, employmentCheck);
                    var employerPayeSchemesTask = context.CallActivityAsync<EmployerPayeSchemes>(payeActivityName, employmentCheck);
                    await Task.WhenAll(learnerNiNumberTask, employerPayeSchemesTask);

                    var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumberTask.Result, employerPayeSchemesTask.Result);
                    if (IsValidEmploymentCheckData(employmentCheckData))
                    {
                        await context.CallActivityAsync(requestActivityName, employmentCheckData);
                    }
                    else
                    {
                        await context.CallActivityAsync(storeActivityName, employmentCheck);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{thisMethodName}: Exception caught - {e.Message}. {e.StackTrace}");
            }
            finally
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Completed.");

                context.ContinueAsNew(null);
            }
        }

        public bool IsValidEmploymentCheckData(EmploymentCheckData employmentCheckData)
        {
            var isValidEmploymentcheckData = true;
            var isValidNino = IsValidNino(employmentCheckData);
            var isValidPayeScheme = IsValidPayeScheme(employmentCheckData);

            if(!isValidNino || !isValidPayeScheme)
            {
                isValidEmploymentcheckData = false;
            }

            return isValidEmploymentcheckData;
        }

        public bool IsValidNino(EmploymentCheckData employmentCheckData)
        {
            const int validNinoLength = 9;
            const string NinoNotFound = "NinoNotFound";
            const string NinoInvalid = "NinoInvalid";
            var isValidNino = true;

            if (employmentCheckData.ApprenticeNiNumber == null ||
                string.IsNullOrEmpty(employmentCheckData.ApprenticeNiNumber.NiNumber))
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoNotFound;
                isValidNino = false;
            }
            else if (employmentCheckData.ApprenticeNiNumber != null &&
                employmentCheckData.ApprenticeNiNumber.NiNumber.Length < validNinoLength)
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoInvalid;
                isValidNino = false;
            }

            return isValidNino;
        }

        public bool IsValidPayeScheme(EmploymentCheckData employmentCheckData)
        {
            const string PayeNotFound = "PAYENotFound";
            const string PayeFailure = "PAYEFailure";
            var isValidPayeScheme = true;
            var existingError = string.Empty;

            if (!string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType))
            {
                existingError = employmentCheckData.EmploymentCheck.ErrorType;
            }

            if (employmentCheckData.EmployerPayeSchemes != null)
            {
                isValidPayeScheme = PayeSchemeValueValidation(employmentCheckData, PayeNotFound, PayeFailure, isValidPayeScheme, existingError);
            }
            else
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(existingError) ? PayeFailure : $"{existingError}And{PayeFailure}";
                isValidPayeScheme = false;
            }

            return isValidPayeScheme;
        }

        public bool PayeSchemeValueValidation(EmploymentCheckData employmentCheckData, string PayeNotFound, string PayeFailure, bool isValidPayeScheme, string existingError)
        {
            if (employmentCheckData.EmployerPayeSchemes.HttpStatusCode == HttpStatusCode.OK)
            {
                if (employmentCheckData.EmployerPayeSchemes.PayeSchemes == null || !employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
                {
                    employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(existingError) ? PayeNotFound : $"{existingError}And{PayeNotFound}";
                    isValidPayeScheme = false;
                }
            }
            else if (employmentCheckData.EmployerPayeSchemes.HttpStatusCode == HttpStatusCode.NotFound)
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(existingError) ? PayeNotFound : $"{existingError}And{PayeNotFound}";
                isValidPayeScheme = false;
            }
            else
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(existingError) ? PayeFailure : $"{existingError}And{PayeFailure}";
                isValidPayeScheme = false;
            }

            return isValidPayeScheme;
        }

        public bool IndividualPayeSchemeValidation(EmploymentCheckData employmentCheckData, string PayeNotFound, bool isValidPayeScheme, string existingError)
        {
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
            foreach (var employerPayeScheme in employmentCheckData.EmployerPayeSchemes.PayeSchemes)
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
            {
                if (string.IsNullOrEmpty(employerPayeScheme))
                {
                    employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(existingError) ? PayeNotFound : $"{existingError}And{PayeNotFound}";
                    isValidPayeScheme = false;
                }
            }

            return isValidPayeScheme;
        }
    }
}