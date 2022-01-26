using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IEmploymentCheckRepository
    {
        Task Save(Application.Models.EmploymentCheck check);

        // InsertOrUpdate - Save/Update, taken from Upsert,  Update/Insert
        // Checks if the value exists first and does an update instead of a save if it already exists
        // (Originally named InsertOrUpdate which didn't sound very 'snappy' so feel free to rename back)
        Task InsertOrUpdate(Models.EmploymentCheck check);
    }
}