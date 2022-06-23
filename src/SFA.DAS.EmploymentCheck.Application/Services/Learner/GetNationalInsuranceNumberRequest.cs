using SFA.DAS.EmploymentCheck.Application.ApiClients;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.Services.Learner
{
    public class GetNationalInsuranceNumberRequest : IGetApiRequest
    {
        private readonly long _uln;
        private readonly string _path;

        public GetNationalInsuranceNumberRequest(long uln, DataCollectionsApiConfiguration configuration)
        {
            _uln = uln;
            _path = configuration.Path.StartsWith("/") ? configuration.Path : string.Concat("/", configuration.Path);
        }

        public string GetUrl => $"{_path}?ulns={_uln}";
    }
}