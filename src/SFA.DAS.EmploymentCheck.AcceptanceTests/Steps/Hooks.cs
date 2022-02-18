//using BoDi;
//using SFA.DAS.EmploymentCheck.AcceptanceTests.DependencyResolution;
//using SFA.DAS.EmploymentCheck.AcceptanceTests.Infrastructure;
//using SFA.DAS.NLog.Logger;
//using System.Threading.Tasks;
//using TechTalk.SpecFlow;

//namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
//{
//    [Binding]
//    public class Hooks
//    {
//        private ILog _log;

//        private readonly IObjectContainer _objectContainer;

//        public Hooks(IObjectContainer objectContainer)
//        {
//            _objectContainer = objectContainer;
//        }

//        [BeforeScenario(Order = 1)]
//        public async Task BeforeScenario()
//        {
//            _log = new NLogLogger();

//            var config = new LocalConfiguration(_log);
//            _objectContainer.RegisterInstanceAs(config);

//            var employmentCheckRepository = new EmploymentCheckRepository(config.Dbconnectionstring, _log);

//            _objectContainer.RegisterInstanceAs(employmentCheckRepository);

//            //Clean Database
//           // await employmentCheckRepository.SetLastProcessedEventId();
//         //   await employmentCheckRepository.RemoveSubmissionEvents();

//        }

//        [AfterScenario(Order = 1)]
//        public void AfterScenario()
//        {
//        }
//    }
//}
