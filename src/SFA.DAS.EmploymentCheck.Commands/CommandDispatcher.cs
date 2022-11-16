using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmploymentCheck.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public class CommandDispatcher : ICommandDispatcher
    {        
        private readonly IServiceProvider _serviceProvider;

        public CommandDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
        {
            var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>();

            if (handler == null)
            {
                throw new CommandDispatcherException($"Unable to dispatch command '{command.GetType().Name}'. No matching handler found.");
            }

            return handler.Handle(command, cancellationToken);
        }

    }
}
