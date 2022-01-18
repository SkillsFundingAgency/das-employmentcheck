using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IEmploymentCheckRepository
    {
        Task Save(Application.Models.EmploymentCheck employmentCheck);
    }
}
