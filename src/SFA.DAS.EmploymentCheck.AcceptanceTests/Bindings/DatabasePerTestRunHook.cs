using SFA.DAS.EmploymentCheck.Tests.Database;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Bindings
{
    [Binding]
    public static class DatabasePerTestRunHook
    {
        [BeforeTestRun(Order = 1)]
        public static void RefreshDatabaseModel()
        {
            SqlDatabaseModel.Update();
        }
    }
}
