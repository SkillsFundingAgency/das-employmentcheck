using NServiceBus;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(PublishEmploymentCheckResultCommand), QueueNames.PublishEmploymentCheckResult);
          
        }
    }
}
