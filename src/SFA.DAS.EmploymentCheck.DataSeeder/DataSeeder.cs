﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.DataSeeder
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
                        var check = new Data.Models.EmploymentCheck
                        {
                            CorrelationId = Guid.NewGuid(),
                            CheckType = Guid.NewGuid().ToString().Replace("-", "")[..20],
                            Uln = Convert.ToInt64(columns[0]),
                            AccountId = Convert.ToInt64(columns[1]),
                            ApprenticeshipId = 12300000 + i,
                            MinDate = Convert.ToDateTime(columns[2]),
                            MaxDate = Convert.ToDateTime(columns[3]),
                            Employed = null,
                            CreatedOn = now
                        };

                        await _dataAccess.Insert(check);
                        Console.WriteLine($"[{i}] Added EmploymentChecks record");

                        if (!_options.SeedEmploymentCheckCacheRequests)
                        {
                            var request = new EmploymentCheckCacheRequest
                            {
                                CorrelationId = check.CorrelationId,
                                Employed = null,
                                ApprenticeEmploymentCheckId = check.Id,
                                MinDate = check.MinDate,
                                MaxDate = check.MaxDate,
                                Nino = columns[4],
                                PayeScheme = Guid.NewGuid().ToString(),
                                RequestCompletionStatus = null,
                                CreatedOn = now,
                            };

                            await _dataAccess.Insert(request);
                            Console.WriteLine($"[{i}] Added EmploymentCheckCacheRequest record");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
            }
        }
        
        private static void ReadSettings()
        {
            _csvDataFile = Path.Combine(Directory.GetCurrentDirectory(), "Files\\testdata.csv");
            if (!File.Exists(_csvDataFile)) throw new FileNotFoundException($"Input file not found in: {_csvDataFile}");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false).Build();

            _options = new Options();
            config.Bind(_options);

            Console.WriteLine($"Number of dataset copies: {_options.DataSets}");
            Console.WriteLine($"Clear existing data: {_options.ClearExistingData}");
            Console.WriteLine($"Seeding [dbo].[EmploymentChecks] table only: {_options.SeedEmploymentCheckCacheRequests}");

            Console.WriteLine($"Using database connection string: {_options.EmploymentChecksConnectionString}");
            _dataAccess = new DataAccess(_options.EmploymentChecksConnectionString);
        }

        private static async Task ClearData()
        {
            await _dataAccess.DeleteAll("[Business].[EmploymentCheck]");
            await _dataAccess.DeleteAll("[Cache].[AccountsResponse]");
            await _dataAccess.DeleteAll("[Cache].[EmploymentCheckCacheRequest]");
            await _dataAccess.DeleteAll("[Cache].[EmploymentCheckCacheResponse]");
            await _dataAccess.DeleteAll("[Cache].[DataCollectionsResponse]");
        }
    }
}