﻿//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
//using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheckMessageQueue;

//namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.EnqueueEmploymentCheckMessages
//{
//    public class EnqueueEmploymentCheckMessagesCommandHandler
//        : IRequestHandler<EnqueueEmploymentCheckMessagesCommand>
//    {
//        private const string ThisClassName = "\n\nEnqueueEmploymentCheckMessagesCommandHandler";

//        private IEmploymentCheckMessageQueueClient _employmentCheckMessageQueueClient;
//        private ILogger<EnqueueEmploymentCheckMessagesCommandHandler> _logger;

//        public EnqueueEmploymentCheckMessagesCommandHandler(
//            IEmploymentCheckMessageQueueClient employmentCheckMessageQueueClient,
//            ILogger<EnqueueEmploymentCheckMessagesCommandHandler> logger)
//        {
//            _employmentCheckMessageQueueClient = employmentCheckMessageQueueClient;
//            _logger = logger;
//        }

//        public async Task<Unit> Handle(
//            EnqueueEmploymentCheckMessagesCommand request,
//            CancellationToken cancellationToken)
//        {
//            var thisMethodName = $"{ThisClassName}.Handle()";

//            try
//            {
//                if (request != null &&
//                    request.ApprenticeRelatedData != null)
//                {
//                    // Call the application client to store the employment check queue messages
//                    await _employmentCheckMessageQueueClient.EnqueueEmploymentCheckMessages_Client(request.ApprenticeRelatedData);
//                }
//                else
//                {
//                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No employment check related data supplied to create queue messaages.");
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
//            }

//            return Unit.Value;
//        }
//    }
//}
