using TechTalk.SpecFlow;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Bindings
{
    [Binding]
    public class EmployerAccountsApi
    {
        private readonly TestContext _context;
        private readonly FeatureInfo _featureInfo;

        public EmployerAccountsApi(TestContext context, FeatureInfo featureInfo)
        {
            _context = context;
            _featureInfo = featureInfo;
        }

        [BeforeScenario(Order = 1)]
        public void Initialise()
        {
            _context.EmployerAccountsApi ??= FeatureTestContext.FeatureData.GetOrCreate<MockApi>(_featureInfo.Title + nameof(EmployerAccountsApi));
        }

        [AfterScenario]
        public void Reset()
        {
            _context.EmployerAccountsApi?.Reset();
        }
    }
}
