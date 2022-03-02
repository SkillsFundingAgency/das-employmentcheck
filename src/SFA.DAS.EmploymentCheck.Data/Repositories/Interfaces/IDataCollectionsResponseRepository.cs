using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IDataCollectionsResponseRepository
    {
        Task InsertOrUpdate(DataCollectionsResponse response);

        Task Save(DataCollectionsResponse response);

        Task<DataCollectionsResponse> Get(DataCollectionsResponse response);

        Task<DataCollectionsResponse> GetByEmploymentCheckId(long apprenticeEmploymentCheckId);
    }
}
