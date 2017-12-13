using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace SFA.DAS.EmploymentCheck.Application.Queries.GetPreviouslyHandledSubmissionEventsFor
{
    public class GetPreviouslyHandledSubmissionEventsForEventRequest : IAsyncRequest<GetPreviouslyHandledSubmissionEventsForEventResponse>
    {
        public string Ulns { get; set; }
    }
}
