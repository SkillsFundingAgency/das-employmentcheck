//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheckMessageQueue;

//namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.EnqueueEmploymentCheckRequests
//{
//    public class EnqueueEmploymentCheckRequestsCommandHandler
//        : IRequestHandler<EnqueueEmploymentCheckRequestsCommand>
//    {
//        private const string ThisClassName = "\n\nEnqueueEmploymentCheckRequestsCommandHandler";

//        private IEmploymentCheckMessageQueueClient _employmentCheckMessageQueueClient;
//        private ILogger<EnqueueEmploymentCheckRequestsCommandHandler> _logger;

//        public EnqueueEmploymentCheckRequestsCommandHandler(
//            IEmploymentCheckMessageQueueClient employmentCheckMessageQueueClient,
//            ILogger<EnqueueEmploymentCheckRequestsCommandHandler> logger)
//        {
//            _employmentCheckMessageQueueClient = employmentCheckMessageQueueClient;
//            _logger = logger;
//        }

//        public async Task<Unit> Handle(
//            EnqueueEmploymentCheckRequestsCommand request,
//            CancellationToken cancellationToken)
//        {
//            var thisMethodName = $"{ThisClassName}.Handle()";

//            try
//            {
//                if (request != null &&
//                    request.EmploymentCheckRequests != null)
//                {
//                    // Call the application client to add the employment check requests to the employment check message queue
//                    //await _employmentCheckMessageQueueClient .EnqueueEmploymentCheckMessages_Client(request.EmploymentCheckRequests);
//                }
//                else
//                {
//                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No employment check requests data supplied to create queue messaages.");
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
