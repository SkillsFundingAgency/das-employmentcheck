using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult
{
    public class PublishEmploymentCheckResultCommandHandler : ICommandHandler<PublishEmploymentCheckResultCommand>
    {
        private readonly ICommandService _service;

        public PublishEmploymentCheckResultCommandHandler(ICommandService service)
        {
            _service = service;
        }

        public Task Handle([NServiceBusTrigger(Endpoint = QueueNames.UpdateVendorRegistrationCaseStatus)] PublishEmploymentCheckResultCommand command, CancellationToken cancellationToken = default)
        {
             _service.Dispatch(command);

             return Task.CompletedTask;
        }
    }
}