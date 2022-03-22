using TechTalk.SpecFlow;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Bindings
{
    [Binding]
    public class HmrcApi
    {
        private readonly TestContext _context;
        private readonly FeatureInfo _featureInfo;

        public HmrcApi(TestContext context, FeatureInfo featureInfo)
        {
            _context = context;
            _featureInfo = featureInfo;
        }

        [BeforeScenario(Order = 1)]
        public void Initialise()
        {
            _context.HmrcApi ??= FeatureTestContext.FeatureData.GetOrCreate<MockApi>(_featureInfo.Title + nameof(HmrcApi));
        }

        [AfterScenario]
        public void Reset()
        {
            _context.HmrcApi?.Reset();
        }
    }
}
