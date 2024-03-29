﻿using Dapper.Contrib.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Cache.RefErrorType")]
    public class RefErrorType
    {
        public RefErrorType() { }

        public RefErrorType(
            long id,
            long employmentCheckId,
            string source,
            string message,
            int maxRetries)
        {
            Id = id;
            EmploymentCheckId = employmentCheckId;
            Source = source;
            Message = message;
            MaxRetries = maxRetries;
        }

        public long Id { get; set; }

        public long EmploymentCheckId { get; set; }

        public string Source { get; set; }

        public string Message { get; set; }

        public int MaxRetries { get; set; }
    }
}

