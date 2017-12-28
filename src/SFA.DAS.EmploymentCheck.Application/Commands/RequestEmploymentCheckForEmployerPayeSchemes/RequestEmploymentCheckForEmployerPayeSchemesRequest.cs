using System;
using MediatR;

namespace SFA.DAS.EmploymentCheck.Application.Commands.RequestEmploymentCheckForEmployerPayeSchemes
{
    public class RequestEmploymentCheckForEmployerPayeSchemesRequest : IAsyncNotification
    {
        public RequestEmploymentCheckForEmployerPayeSchemesRequest(string nationalInsuranceNumber, long uln, long employerAccountId, long ukprn, DateTime actualStartDate)
        {
            NationalInsuranceNumber = nationalInsuranceNumber;
            Uln = uln;
            EmployerAccountId = employerAccountId;
            Ukprn = ukprn;
            ActualStartDate = actualStartDate;
        }

        public string NationalInsuranceNumber { get; }
        public long Uln { get; }
        public long EmployerAccountId { get; }
        public long Ukprn { get; }
        public DateTime ActualStartDate { get; }
    }
}
