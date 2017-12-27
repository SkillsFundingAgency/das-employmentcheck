using System;
using System.Collections.Generic;
using MediatR;

namespace SFA.DAS.EmploymentCheck.Application.Commands.PerformEmploymentCheck
{
    public class PerformEmploymentCheckRequest : IAsyncNotification
    {
        public PerformEmploymentCheckRequest(string nationalInsuranceNumber, long uln, long employerAccountId, long ukprn, DateTime actualStartDate, IEnumerable<string> payeSchemes)
        {
            NationalInsuranceNumber = nationalInsuranceNumber;
            Uln = uln;
            EmployerAccountId = employerAccountId;
            Ukprn = ukprn;
            ActualStartDate = actualStartDate;
            PayeSchemes = payeSchemes;
        }

        public string NationalInsuranceNumber { get; }
        public long Uln { get; }
        public long EmployerAccountId { get; }
        public long Ukprn { get; }
        public DateTime ActualStartDate { get; }
        public IEnumerable<string> PayeSchemes { get; }
    }
}
