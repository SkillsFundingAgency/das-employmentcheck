using System.Text.Json.Serialization;

namespace SFA.DAS.EmploymentCheck.Application.ApiClients
{
    public interface IGetApiRequest
    {
        [JsonIgnore]
        string GetUrl { get; }
    }
}
