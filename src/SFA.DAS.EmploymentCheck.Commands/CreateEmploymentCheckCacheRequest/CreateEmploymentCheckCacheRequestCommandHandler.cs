using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Domain.Enums;

namespace SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest
{
    public class CreateEmploymentCheckCacheRequestCommandHandler
        : ICommandHandler<CreateEmploymentCheckCacheRequestCommand>
    {
        private readonly IEmploymentCheckService _service;

        public CreateEmploymentCheckCacheRequestCommandHandler(IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task Handle(
            CreateEmploymentCheckCacheRequestCommand request,
            CancellationToken cancellationToken)
        {
            if (IsValid(request))
            {
                await _service.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData);
            }
            else
            {
                request.EmploymentCheckData.EmploymentCheck.SetRequestCompletionStatus(ProcessingCompletionStatus.Completed);
               
                await _service.SaveEmploymentCheck(request.EmploymentCheckData.EmploymentCheck);
            }
        }

        private static bool IsValid(CreateEmploymentCheckCacheRequestCommand request)
        {
            return request.EmploymentCheckData.ApprenticeNiNumber != null
                   && !string.IsNullOrEmpty(request.EmploymentCheckData.ApprenticeNiNumber.NiNumber)
                   && request.EmploymentCheckData.EmployerPayeSchemes != null
                   && request.EmploymentCheckData.EmployerPayeSchemes.PayeSchemes != null
                   && request.EmploymentCheckData.EmployerPayeSchemes.PayeSchemes.Any();
        }
    }
}