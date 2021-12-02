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
        private string[] _accounts;
        private string[] _learners;

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

            var accountsFile = hdDirectoryInWhichToSearch + "\\accounts.txt";
            if (File.Exists(accountsFile))
            {
                Console.WriteLine($"Found accounts list file: {accountsFile}");
                _accounts = File.ReadAllLines(accountsFile);
            }

            var learnersFile = hdDirectoryInWhichToSearch + "\\learners.txt";
            if (File.Exists(learnersFile))
            {
                Console.WriteLine($"Found learners list file: {learnersFile}");
                _learners = File.ReadAllLines(learnersFile);
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
                var now = DateTime.Now;

                var check = new EmploymentChecks
                {
                    ULN = Convert.ToInt64(_learners[i % _learners.Length].Split('\t')[0]),
                    ApprenticeshipId = 122 + i,
                    UKPRN = 10000000 + i,
                   // NationalInsuranceNumber = _learners[i % _learners.Length].Split('\t')[1],//data.jsonBody.nino,
                    AccountId = Convert.ToInt64(_accounts[i % _accounts.Length]),
                    MinDate = data.jsonBody.fromDate,
                    MaxDate = data.jsonBody.toDate,
                    CheckType = "StartDate+60",
                    IsEmployed = null,
                    HasBeenChecked = false,
                    CreatedDate = now,
                    LastUpdated = now
                };
                
                var checkId = await _dataAccess.Insert(check);

                if (_options.SeedEmploymentChecksOnly) continue;
                
                var queue = new ApprenticeEmploymentCheckMessageQueue
                {
                    MessageId =  Guid.NewGuid(),
                    MessageCreatedDateTime = now,
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
