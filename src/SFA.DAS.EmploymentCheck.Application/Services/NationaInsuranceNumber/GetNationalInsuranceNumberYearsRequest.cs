using SFA.DAS.EmploymentCheck.Application.ApiClients;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber
{
    public class GetNationalInsuranceNumberYearsRequest : IGetApiRequest
    {
        private readonly string _path;

        public GetNationalInsuranceNumberYearsRequest(DataCollectionsApiConfiguration configuration)
        {
            _path = configuration.AcademicYearsPath.StartsWith("/") ? configuration.AcademicYearsPath.Trim() : string.Concat("/", configuration.AcademicYearsPath.Trim());
        }

        public string GetUrl => $"{_path}";
    }
}