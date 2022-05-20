using SFA.DAS.EmploymentCheck.Application.ApiClients;
using System;

namespace SFA.DAS.EmploymentCheck.Application.Services.Learner
{
    public class GetNationalInsuranceNumberRequest : IGetApiRequest
    {
        private readonly long _uln;

        public GetNationalInsuranceNumberRequest(long uln)
        {
            _uln = uln;
        }

        public string GetUrl => $"/api/v1/ilr-data/learnersNi/{GetAccademicYear(DateTime.Now)}?ulns={_uln}";

        public static int GetAccademicYear(DateTime dtCurrent)
        {
            int year = dtCurrent.Year;
            if (dtCurrent.Month >= 9)
                year++;

            return year;
        }
    }
}