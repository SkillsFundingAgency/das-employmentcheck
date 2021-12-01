namespace app_levy_data_seeder
{
    public class Options
    {
        public string EmploymentChecksConnectionString { get; set; }
        public string DataFolderLocation { get; set; }
        public int DataSets { get; set; }
        public bool ClearExistingData { get; set; }
    }
}
