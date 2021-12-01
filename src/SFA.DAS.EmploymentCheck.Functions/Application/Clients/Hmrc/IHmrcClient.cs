using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc
{
    public interface IHmrcClient
    {
        Task<EmploymentCheckMessage> CheckEmploymentStatus_Client(EmploymentCheckMessage employmentCheckMessage);
    }
}
