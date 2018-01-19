using BoDi;
using TechTalk.SpecFlow;
using SubmissionEventWorkerRole;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
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
            _objectContainer.RegisterInstanceAs(new TestContext().Client);
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
