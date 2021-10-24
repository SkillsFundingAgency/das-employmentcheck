using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public interface IEmploymentCheckClient
    {
        Task<IList<Apprentice>> GetApprentices();
    }
}
