using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IDataCollectionsResponseRepository
    {
        Task InsertOrUpdate(DataCollectionsResponse response);

        Task Save(DataCollectionsResponse response);

        Task<DataCollectionsResponse> Get(DataCollectionsResponse response);
    }
}
