using AutoMapper;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using SFA.DAS.EmploymentCheck.Application.Common.Interfaces;

namespace SFA.DAS.EmploymentCheck.Application.ApprenticeEmploymentChecks.Queries.GetApprenticeEmploymentChecks.Models
{
    public class GetEmploymentChecksQueryHandler
        : IRequestHandler<GetEmploymentChecksQueryRequest,
            GetEmploymentCheckQueryResult>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetEmploymentChecksQueryHandler(
            IApplicationDbContext dbContext,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<GetEmploymentCheckQueryResult> Handle(
            GetEmploymentChecksQueryRequest getEmploymentChecksQueryRequest,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(getEmploymentChecksQueryRequest, nameof(getEmploymentChecksQueryRequest));

            //var apprenticeEmploymentCheckDtos = await _dbContext.ApprenticeEmploymentChecks;


            //var apprenticeEmploymentCheckDtos = await _dbContext.ApprenticeEmploymentChecks.FirstOrDefault();

            //.Where(x => x.Id > getEmploymentChecksQueryRequest.LastHighestBatchId).Take(10) // TODO: Replace hard-coded batch-size with configuration
            //.OrderBy(x => x.Id);
            //.ProjectTo<ApprenticeEmploymentCheckDto>(_mapper.ConfigurationProvider);
            //Guard.Against.Null(apprenticeEmploymentCheckDtos, nameof(apprenticeEmploymentCheckDtos));

            //return new GetEmploymentCheckQueryResult(apprenticeEmploymentCheckDtos);
            return new GetEmploymentCheckQueryResult(null);
        }
    }
}
