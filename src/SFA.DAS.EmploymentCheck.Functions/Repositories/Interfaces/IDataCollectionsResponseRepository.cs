using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Repositories.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IDataCollectionsResponseRepository
   //     : IRepository<DataCollectionsResponse>
    {
        Task InsertOrUpdate(DataCollectionsResponse response);

        Task Save(DataCollectionsResponse response);

        Task<DataCollectionsResponse> Get(DataCollectionsResponse response);
    }
}
