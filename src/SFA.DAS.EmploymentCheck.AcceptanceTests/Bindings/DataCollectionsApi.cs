using TechTalk.SpecFlow;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Bindings
{
    [Binding]
    public class DataCollectionsApi
    {
        private readonly TestContext _context;
        private readonly FeatureInfo _featureInfo;

        public DataCollectionsApi(TestContext context, FeatureInfo featureInfo)
        {
            _context = context;
            _featureInfo = featureInfo;
        }

        [BeforeScenario(Order = 1)]
        public void Initialise()
        {
            _context.DataCollectionsApi ??= FeatureTestContext.FeatureData.GetOrCreate<MockApi>(_featureInfo.Title + nameof(DataCollectionsApi));
            _context.DataCollectionsApiConfiguration.BaseUrl = _context.DataCollectionsApi.BaseAddress;
            _context.DataCollectionsApiConfiguration.Path = "/api/v1/ilr-data/learnersNi/2122";
        }

        [AfterScenario]
        public void Reset()
        {
            _context.DataCollectionsApi?.Reset();
        }
    }
}
