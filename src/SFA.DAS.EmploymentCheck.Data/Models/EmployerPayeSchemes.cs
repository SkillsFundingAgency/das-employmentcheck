using System.Collections.Generic;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class EmployerPayeSchemes
    {
        public EmployerPayeSchemes() { }

        public EmployerPayeSchemes(
            long employerAccountId,
            IList<string> payeSchemes,
            HttpStatusCode httpStatusCode
        )
        {
            EmployerAccountId = employerAccountId;
            PayeSchemes = payeSchemes;
            HttpStatusCode = httpStatusCode;
        }

        public long EmployerAccountId { get; set; }

        public IList<string> PayeSchemes { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
