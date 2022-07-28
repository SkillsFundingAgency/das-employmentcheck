namespace SFA.DAS.EmploymentCheck.DataSeeder
{
    public class Options
    {
        public string EmploymentChecksConnectionString { get; set; }
        public int DataSets { get; set; }
        public bool ClearExistingData { get; set; }
        public bool SeedEmploymentCheckCacheRequests { get; set; } = false;
    }
}
