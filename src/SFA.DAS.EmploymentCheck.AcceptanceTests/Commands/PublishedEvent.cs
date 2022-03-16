namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Commands
{
    public class PublishedEvent
    {
        public object Event { get; }
        public bool IsReceived { get; set; }
        public bool IsProcessed { get; set; }
        public PublishedEvent(object @event)
        {
            Event = @event;
        }
    }
}
