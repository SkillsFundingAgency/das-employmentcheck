using System.Collections.Generic;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class EmployerPayeSchemes
    {
        public EmployerPayeSchemes(long employerAccountId, HttpStatusCode httpStatusCode, IList<string> payeSchemes = null)
        {
            EmployerAccountId = employerAccountId;
            HttpStatusCode = httpStatusCode;
            PayeSchemes = payeSchemes ?? new List<string>();
        }

        public long EmployerAccountId { get; set; }

        public IList<string> PayeSchemes { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
