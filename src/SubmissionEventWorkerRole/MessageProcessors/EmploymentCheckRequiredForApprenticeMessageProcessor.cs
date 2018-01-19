using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Application.Commands.RequestEmploymentCheckForEmployerPayeSchemes;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Messaging;
using SFA.DAS.Messaging.AzureServiceBus.Attributes;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmploymentCheck.SubmissionEventWorkerRole.MessageProcessors
{
    [TopicSubscription("EmploymentCheck_CheckRequiredForApprentice")]
    public class EmploymentCheckRequiredForApprenticeMessageProcessor : MessageProcessor<EmploymentCheckRequiredForApprenticeMessage>
    {
        private readonly IMediator _mediator;

        public EmploymentCheckRequiredForApprenticeMessageProcessor(IMessageSubscriberFactory subscriberFactory, ILog log, IMediator mediator) : base(subscriberFactory, log)
        {
            _mediator = mediator;
        }

        protected override async Task ProcessMessage(EmploymentCheckRequiredForApprenticeMessage messageContent)
        {
            var request = new RequestEmploymentCheckForEmployerPayeSchemesRequest
            (
                messageContent.NationalInsuranceNumber, messageContent.Uln, messageContent.EmployerAccountId, messageContent.Ukprn, messageContent.ActualStartDate
            );
            await _mediator.PublishAsync(request);
        }
    }
}
