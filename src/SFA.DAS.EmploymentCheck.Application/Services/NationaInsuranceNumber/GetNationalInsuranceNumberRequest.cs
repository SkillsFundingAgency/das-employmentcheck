using SFA.DAS.EmploymentCheck.Application.ApiClients;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber
{
    public class GetNationalInsuranceNumberRequest : IGetApiRequest
    {
        private readonly long _uln;
        private readonly string _academicYear;
        private readonly string _path;

        public GetNationalInsuranceNumberRequest(long uln, string academicYear, DataCollectionsApiConfiguration configuration)
        {
            _uln = uln;
            _academicYear = academicYear.Trim();
            _path = configuration.Path.StartsWith("/") ? configuration.Path.Trim() : string.Concat("/", configuration.Path.Trim());
        }

        public string GetUrl => $"{_path}/{_academicYear}?ulns={_uln}";
    }
}