using System;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Models.Dtos
{
    public class EmployerPayeSchemesDto
    {
        public EmployerPayeSchemesDto() { }

        public EmployerPayeSchemesDto(
            long accountId,
            IList<string> payeSchemes)
        {
            AccountId = accountId;
            PayeSchemes = payeSchemes;
        }

        public long AccountId { get; set; }

        public IList<string> PayeSchemes { get; set; }
    }
}
