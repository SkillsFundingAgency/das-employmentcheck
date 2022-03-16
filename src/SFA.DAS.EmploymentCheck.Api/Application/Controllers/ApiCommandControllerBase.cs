using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmploymentCheck.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiCommandControllerBase : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        protected ApiCommandControllerBase(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        protected Task SendCommandAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            return _commandDispatcher.Send(command);
        }

    }
}