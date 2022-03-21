using SFA.DAS.EmploymentCheck.Abstractions;
using System;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Commands
{
    public class PublishedCommand
    {
        public ICommand Command { get; }
        public bool IsReceived { get; set; }
        public bool IsPublished { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsDelayed { get; set; }
        public bool IsDomainCommand { get; set; }
        public bool IsPublishedWithNoListener { get; set; }
        public bool IsErrored { get; set; }
        public Exception LastError { get; set; }

        public PublishedCommand(ICommand command)
        {
            Command = command;
        }
    }
}
