using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;

namespace SFA.DAS.EmploymentCheck.Application.Queries.GetLastKnownProcessedSubmissionEvent
{
    public class GetLastKnownProcessedSubmissionEventHandler : IAsyncRequestHandler<GetLastKnownProcessedSubmissionEventRequest, GetLastKnownProcessedSubmissionEventResponse>
    {
        private readonly ISubmissionEventRepository _repository;

        public GetLastKnownProcessedSubmissionEventHandler(ISubmissionEventRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository), "Ensure that the ioc mapping is registered for the ISubmissionEventRepository");
            }
            _repository = repository;
        }

        public async Task<GetLastKnownProcessedSubmissionEventResponse> Handle(GetLastKnownProcessedSubmissionEventRequest message)
        {
            var lastProcessedId = await _repository.GetPollingProcessingStartingPoint();

            return new GetLastKnownProcessedSubmissionEventResponse
            {
                EventId = lastProcessedId
            };
        }
    }
}
