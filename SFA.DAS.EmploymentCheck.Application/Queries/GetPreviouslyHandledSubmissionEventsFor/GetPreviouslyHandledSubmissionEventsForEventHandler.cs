using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;

namespace SFA.DAS.EmploymentCheck.Application.Queries.GetPreviouslyHandledSubmissionEventsFor
{
    public class GetPreviouslyHandledSubmissionEventsForEventHandler : IAsyncRequestHandler<GetPreviouslyHandledSubmissionEventsForEventRequest, GetPreviouslyHandledSubmissionEventsForEventResponse>
    {
        private readonly ISubmissionEventRepository _repository;

        public GetPreviouslyHandledSubmissionEventsForEventHandler(ISubmissionEventRepository repository)
        {
            _repository = repository;
        }

        public Task<GetPreviouslyHandledSubmissionEventsForEventResponse> Handle(GetPreviouslyHandledSubmissionEventsForEventRequest message)
        {
            throw new NotImplementedException();
        }
    }
}
