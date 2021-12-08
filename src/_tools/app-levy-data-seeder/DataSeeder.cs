using app_levy_data_seeder.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace app_levy_data_seeder
{
    public class DataSeeder
    {
        private static DataAccess _dataAccess;
        private static Options _options;
        private static string _csvDataFile;

        public DataSeeder()
        {
            ReadSettings();
            Console.WriteLine();

            Console.WriteLine("Confirm these settings are correct by pressing [enter]:");
            Console.ReadLine();
        }
      
        public async Task DoTheWork()
        {
            Console.WriteLine("Seeding data...");
            Console.WriteLine();
            await SeedData();
        }

        public async Task SeedData()
        {
            try
            {
                var i = 0;


                if (_options.ClearExistingData) await ClearData();
                var now = DateTime.Now;


                for (var j = 0; j < _options.DataSets; j++)
                {
                    foreach (var line in File.ReadLines(_csvDataFile).Skip(1))
                    {
                        i++;
                        var columns = line.Split(',');
                        var check = new EmploymentChecks
                        {
                            ULN = Convert.ToInt64(columns[0]),
                            AccountId = Convert.ToInt64(columns[1]),
                            MinDate = Convert.ToDateTime(columns[2]),
                            MaxDate = Convert.ToDateTime(columns[3]),
                            HasBeenChecked = false,
                            CreatedDate = now,
                            CheckType = Guid.NewGuid().ToString().Replace("-", "")[..20]
                        };

                        var checkId = await _dataAccess.Insert(check);
                        Console.WriteLine($"[{i}] Added EmploymentChecks record");

                        if (_options.SeedEmploymentChecksOnly) continue;

                        var queue = new ApprenticeEmploymentCheckMessageQueue
                        {
                            MessageId = Guid.NewGuid(),
                            MessageCreatedDateTime = now,
                            EmploymentCheckId = checkId,
                            Uln = check.ULN,
                            NationalInsuranceNumber = columns[4],
                            PayeScheme = columns[5],
                            StartDateTime = check.MinDate,
                            EndDateTime = check.MaxDate
                        };

                        await _dataAccess.Insert(queue);
                        Console.WriteLine($"[{i}] Added ApprenticeEmploymentCheckMessageQueue record");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
            }
        }

        private static void ReadSettings()
        {
            _csvDataFile = Path.Combine(Directory.GetCurrentDirectory(), "Files\\testdata.csv");
            if (!File.Exists(_csvDataFile)) throw new Exception($"Input file not found in: {_csvDataFile}");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false).Build();

            _options = new Options();
            config.Bind(_options);

            Console.WriteLine($"Number of dataset copies: {_options.DataSets}");
            Console.WriteLine($"Clear existing data: {_options.ClearExistingData}");
            Console.WriteLine($"Seeding [dbo].[EmploymentChecks] table only: {_options.SeedEmploymentChecksOnly}");

            Console.WriteLine($"Using database connection string: {_options.EmploymentChecksConnectionString}");
            _dataAccess = new DataAccess(_options.EmploymentChecksConnectionString);
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
