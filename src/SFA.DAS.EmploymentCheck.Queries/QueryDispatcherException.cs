using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SFA.DAS.EmploymentCheck.Queries
{
    [Serializable]
    public sealed class QueryDispatcherException : Exception
    {
        public QueryDispatcherException()
        {
        }

        public QueryDispatcherException(string message)
            : base(message)
        {
        }

        public QueryDispatcherException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        private QueryDispatcherException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
   
}
