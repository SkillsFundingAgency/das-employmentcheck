using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NServiceBus;
using SFA.DAS.EmploymentCheck.Functions.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Messages;

namespace SFA.DAS.EmploymentCheck.Functions.Commands.InitiateEmploymentCheck
{
    public class InitiateEmploymentCheckHandler : IRequestHandler<InitiateEmploymentCheckRequest>
    {
        private readonly IMessageSession _eventPublisher;

        public InitiateEmploymentCheckHandler(IMessageSession eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public async Task<Unit> Handle(InitiateEmploymentCheckRequest request, CancellationToken cancellationToken)
        {
            var apprentices = new List<ApprenticeToVerifyDto> { new ApprenticeToVerifyDto("ABC123", "ssdjhgffg", 34354, 4356456, DateTime.Now ) };

            foreach (var apprentice in apprentices)
            {
                await _eventPublisher.Publish(new EmploymentCheckForApprenticeRequired(apprentice.ULN, apprentice.NationalInsuranceNumber, apprentice.ActualStartDate, apprentice.HashedAccountId, apprentice.UKPRN));
            }

            return Unit.Value;
        }
    }
}
