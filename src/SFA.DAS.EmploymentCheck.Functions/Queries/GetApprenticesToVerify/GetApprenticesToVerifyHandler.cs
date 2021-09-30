using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Functions.DataAccess;

namespace SFA.DAS.EmploymentCheck.Functions.Queries.GetApprenticesToVerify
{
    public class GetApprenticesToVerifyHandler : IRequestHandler<GetApprenticesToVerifyRequest, GetApprenticesToVerifyResult>
    {
        private IEmploymentChecksRepository _repository;

        public GetApprenticesToVerifyHandler(IEmploymentChecksRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetApprenticesToVerifyResult> Handle(GetApprenticesToVerifyRequest request, CancellationToken cancellationToken)
        {
            var apprenticesToCheck = await _repository.GetApprenticesToCheck();
            return new GetApprenticesToVerifyResult(apprenticesToCheck);
        }
    }
}
