using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc
{
    public interface IHmrcClient
    {
        Task<ApprenticeEmploymentCheckMessageModel> CheckApprenticeEmploymentStatus_Client(ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel);
    }
}
