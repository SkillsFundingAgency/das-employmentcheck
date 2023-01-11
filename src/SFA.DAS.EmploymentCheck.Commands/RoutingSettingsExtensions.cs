using NServiceBus;
using SFA.DAS.EmploymentCheck.Types;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings settings)
        {
            settings.RouteToEndpoint(typeof(EmploymentCheckCompletedEvent), QueueNames.PublishEmploymentCheckResult);
        }
    }
}
