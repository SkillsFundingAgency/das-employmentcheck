using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Commands.Types;
using System.Linq;
using System.Reflection;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests.ServiceBus
{
    public class WhenDomainMessageQueueName
    {
        // Fix for ServiceBus error
        // The listener for function 'HandleXCommand' was unable to start.
        // Entity path 'SFA.DAS.EmployerIncentives.QueueName' exceeds the '50' character limit. (Parameter 'SubscriptionName')
        [Test]
        public void Then_its_name_does_not_exceed_50_characters()
        {
            var queues = (typeof(QueueNames)).GetFields(BindingFlags.Public | BindingFlags.Static |
                                                        BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(x => x.GetRawConstantValue()).ToList();

            Assert.IsTrue(queues.Any());
            queues.ForEach(q => Assert.IsTrue(((string)q).Length <= 50, $"'{q}' is {((string)q).Length} long and therefore exceeds 50 character limit"));
        }
    }
}