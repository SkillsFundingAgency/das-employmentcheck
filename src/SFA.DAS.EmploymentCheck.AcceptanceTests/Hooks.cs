using TechTalk.SpecFlow;
using BoDi;
using SFA.DAS.EmploymentCheck.SubmissionEventWorkerRole;

namespace SFA.DAS.EmploymentCheck.UserAcceptanceTests
{
    [Binding]
    public class Hooks
    {
        private IObjectContainer _objectContainer;

        private WorkerRole sut;

        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            sut = new WorkerRole();
            sut.OnStart();
            sut.Run();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            sut.OnStop();
        }
    }
}
