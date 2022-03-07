using SFA.DAS.EmploymentCheck.Application.ApiClients;

namespace SFA.DAS.EmploymentCheck.Application.Services.Learner
{
    public class GetNationalInsuranceNumberRequest : IGetApiRequest
    {
        private readonly long _uln;

        public GetNationalInsuranceNumberRequest(long uln)
        {
            _uln = uln;
        }

        public string GetUrl => $"/api/v1/ilr-data/learnersNi/2122?ulns={_uln}";
    }
}