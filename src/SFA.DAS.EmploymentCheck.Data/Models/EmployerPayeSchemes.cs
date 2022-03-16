using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class EmployerPayeSchemes
    {
        public EmployerPayeSchemes(long employerAccountId, IList<string> payeSchemes = null)
        {
            EmployerAccountId = employerAccountId;
            PayeSchemes = payeSchemes ?? new List<string>();
        }

        public long EmployerAccountId { get; set; }

        public IList<string> PayeSchemes { get; set; }
    }
}
