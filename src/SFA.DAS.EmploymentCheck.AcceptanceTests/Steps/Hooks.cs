using System.Threading.Tasks;
using TechTalk.SpecFlow;
using BoDi;
using SFA.DAS.EmploymentCheck.SubmissionEventWorkerRole;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Infrastructure;
using SFA.DAS.EmploymentCheck.AcceptanceTests.DependencyResolution;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    public class Hooks
    {
        private ILog _log;

        private IObjectContainer _objectContainer;

        private WorkerRole sut;
        
        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }
        
        [BeforeScenario(Order = 1)]
        public async Task BeforeScenario()
        {
            _log = new NLogLogger();

            var config = new LocalConfiguration(_log);
            _objectContainer.RegisterInstanceAs(config);

            EmploymentCheckRepository employmentCheckRepository = new EmploymentCheckRepository(config.Dbconnectionstring,_log);

            sut = new WorkerRole();
            sut.OnStart();

            _objectContainer.RegisterInstanceAs(sut);
           
            _objectContainer.RegisterInstanceAs(employmentCheckRepository);

            //Clean Database
            await employmentCheckRepository.SetLastProcessedEventId();
            await employmentCheckRepository.RemoveSubmissionEvents();
            
        }

        [AfterScenario(Order = 1)]
        public void AfterScenario()
        {
            sut?.OnStop();
        }
    }
}
