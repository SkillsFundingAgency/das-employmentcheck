using SFA.DAS.EAS.Account.Api.Types;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount
{
    public interface IEmployerAccountService
    {
        Task<ResourceList> GetPayeSchemes(Models.EmploymentCheck apprenticeEmploymentCheck);
    }
}
