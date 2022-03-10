using System;

namespace SFA.DAS.EmploymentCheck.Queries
{
    [Serializable]
    public sealed class QueryException : Exception
    {
        public QueryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}
