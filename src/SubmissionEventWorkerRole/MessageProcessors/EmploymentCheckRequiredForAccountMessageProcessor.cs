using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Application.Commands.PerformEmploymentCheck;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Messaging;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SubmissionEventWorkerRole.MessageProcessors
{
    public class EmploymentCheckRequiredForAccountMessageProcessor : MessageProcessor<EmploymentCheckRequiredForAccountMessage>
    {
        private readonly IMediator _mediator;

        public EmploymentCheckRequiredForAccountMessageProcessor(IMessageSubscriberFactory subscriberFactory, ILog log, IMediator mediator) : base(subscriberFactory, log)
        {
            _mediator = mediator;
        }

        protected override async Task ProcessMessage(EmploymentCheckRequiredForAccountMessage messageContent)
        {
            await _mediator.PublishAsync(new PerformEmploymentCheckRequest
            (
                messageContent.NationalInsuranceNumber, messageContent.Uln, messageContent.EmployerAccountId, messageContent.Ukprn, messageContent.ActualStartDate, messageContent.PayeSchemes
            ));
        }
    }
}
