using System;
using System.Diagnostics;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Database;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Bindings
{
    [Binding]
    public class DatabasePerScenarioHook
    {
        [BeforeScenario(Order = 2)]
        public void CreateDatabase(TestContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            context.SqlDatabase = new SqlDatabase("SFA.DAS.EmploymentCheck.Database");
           // context.SqlDatabase = new SqlDatabase(context.InstanceId);
            stopwatch.Stop();
            Console.WriteLine($@"[{nameof(DatabasePerScenarioHook)}] time it took to deploy test database: {stopwatch.Elapsed.Seconds} seconds");
        }

        [AfterScenario(Order = 100)]
        public static void TearDownDatabase(TestContext context)
        {
            // context.SqlDatabase?.Dispose();
        }
    }
}
