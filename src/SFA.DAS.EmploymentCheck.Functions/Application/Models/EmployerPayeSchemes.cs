﻿using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    public class EmployerPayeSchemes
    {
        public EmployerPayeSchemes() { }

        public EmployerPayeSchemes(
            long employerAccountId,
            IList<string> payeSchemes)
        {
            EmployerAccountId = employerAccountId;
            PayeSchemes = payeSchemes;
        }

        [Key]
        public long EmployerAccountId { get; set; }

        public IList<string> PayeSchemes { get; set; }
    }
}
