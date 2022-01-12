using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace SFA.DAS.EmploymentCheck.Api.Commands
{
    public class PostRegisterCheckCommandHandler : IRequestHandler<PostRegisterCheckCommand, PostRegisterCheckResponse>
    {
        public async Task<PostRegisterCheckResponse> Handle(PostRegisterCheckCommand command,
            CancellationToken cancellationToken)
        {
            return null;
        }
    }
}