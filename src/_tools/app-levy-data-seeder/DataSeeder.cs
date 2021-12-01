using app_levy_data_seeder.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace app_levy_data_seeder
{
    public class DataSeeder
    {
        public IList<InputData> SourceData = new List<InputData>();
        private static DataAccess _dataAccess;
        private static Options _options;

        public DataSeeder()
        {
            ReadSettings();
            Console.WriteLine();

            Console.WriteLine("Confirm these settings are correct by pressing [enter]:");
            Console.ReadLine();
        }
      
        public async Task DoTheWork()
        {
            Console.WriteLine("Reading data...");
            Console.WriteLine();

            ReadSourceData();
            await SeedData();
        }

        public void ReadSourceData()
        {
            var hdDirectoryInWhichToSearch = new DirectoryInfo(_options.DataFolderLocation);
            var dirsInDir = hdDirectoryInWhichToSearch.GetDirectories("*-employed-*");

            for (var i = 0; i < _options.DataSets; i++)
            {
                foreach (var foundDir in dirsInDir)
                {
                    var files = foundDir.GetDirectories().First().GetDirectories().First().GetDirectories().First()
                        .GetDirectories()
                        .First().GetFiles("*.json");

                    foreach (var file in files)
                    {
                        Console.WriteLine($"Found data file: {file.FullName}");
                        var data = JsonConvert.DeserializeObject<InputData>(File.ReadAllText(file.FullName));
                        SourceData.Add(data);
                    }
                }
            }
        }

        private static void ReadSettings()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false).Build();

            _options = new Options();
            config.Bind(_options);

            Console.WriteLine($"Using database connection string: {_options.EmploymentChecksConnectionString}");
            _dataAccess = new DataAccess(_options.EmploymentChecksConnectionString);

            if (!Directory.Exists(_options.DataFolderLocation)) throw new Exception($"Cannot find data folder here: {_options.DataFolderLocation}");
            Console.WriteLine($"Using data folder: {_options.DataFolderLocation}");

            Console.WriteLine($"Number of dataset copies: {_options.DataSets}");
            Console.WriteLine($"Clear existing data: {_options.ClearExistingData}");
            Console.WriteLine($"Seeding [dbo].[EmploymentChecks] table only: {_options.SeedEmploymentChecksOnly}");
        }

        public  async Task SeedData()
        {
            if (_options.ClearExistingData) await ClearData();
            await InsertData();
        }

        private async Task InsertData()
        {
            var i = 0;
            foreach (var data in SourceData)
            {
                i++;

                var check = new EmploymentChecks
                {
                    ULN = 1000000000 + i,
                    ApprenticeshipId = 122 + i,
                    UKPRN = 10000000 + i,
                    AccountId = i,
                    MinDate = data.jsonBody.fromDate,
                    MaxDate = data.jsonBody.toDate,
                    CheckType = "StartDate+60",
                    IsEmployed = null,
                    HasBeenChecked = false,
                    CreatedDate = DateTime.Now
                };

                var checkId = await _dataAccess.Insert(check);

                if (_options.SeedEmploymentChecksOnly) continue;
                
                var queue = new ApprenticeEmploymentCheckMessageQueue
                {
                    MessageId =  Guid.NewGuid(),
                    MessageCreatedDateTime = DateTime.Now,
                    EmploymentCheckId = checkId,
                    Uln = check.ULN,
                    NationalInsuranceNumber = data.jsonBody.nino,
                    PayeScheme = data.jsonBody.empref.ToUpper(),
                    StartDateTime = check.MinDate,
                    EndDateTime = check.MaxDate
                };

                await _dataAccess.Insert(queue);
            }
        }


        private static async Task ClearData()
        {
           await _dataAccess.DeleteAll("[dbo].[ApprenticeEmploymentCheckMessageQueueHistory]");
           await _dataAccess.DeleteAll("[dbo].[ApprenticeEmploymentCheckMessageQueue]");
           await _dataAccess.DeleteAll("[dbo].[EmploymentChecks]");
           await _dataAccess.DeleteAll("[dbo].[EmploymentChecksControlTable]");
           await _dataAccess.DeleteAll("[dbo].[ExecutionTrace]");
           await _dataAccess.DeleteAll("[employer_check].[DAS_SubmissionEvents]");
           await _dataAccess.DeleteAll("[employer_check].[LastProcessedEvent]");
        }

        
    }
}
