using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Repositories;

public interface IEmploymentCheckRepository
{
    Task<Application.Models.EmploymentCheck> GetEmploymentCheck(Guid correlationId);
    Task Insert(Application.Models.EmploymentCheck employmentCheck);
    Task<IReadOnlyList<Application.Models.EmploymentCheck>> GetLatestChecksByApprenticeshipIds(IReadOnlyList<long> apprenticeshipIds);
}