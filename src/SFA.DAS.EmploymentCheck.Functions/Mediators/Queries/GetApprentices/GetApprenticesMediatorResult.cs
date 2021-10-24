using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprentices
{
    public class GetApprenticesMediatorResult
    {
        public GetApprenticesMediatorResult(IList<Apprentice> apprentices)
        {
            Apprentices = apprentices;
        }

        public IList<Apprentice> Apprentices { get; }
    }
}
