using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Domain.Models;

namespace SFA.DAS.EmploymentCheck.Application.Queries.GetPreviouslyHandledSubmissionEventsFor
{
    public class GetPreviouslyHandledSubmissionEventsForEventResponse
    {
       public IEnumerable<PreviousHandledSubmissionEvent> PreviouslyHandledSubmissionEvents { get; set; }
    }
}
