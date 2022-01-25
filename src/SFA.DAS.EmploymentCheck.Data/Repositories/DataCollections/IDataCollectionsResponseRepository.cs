using SFA.DAS.EmploymentCheck.Data.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public interface IDataCollectionsResponseRepository
    {
        Task Save(DataCollectionsResponse dataCollectionsResponse);

        Task<DataCollectionsResponse> Get(DataCollectionsResponse dataCollectionsResponse);
    }
}
