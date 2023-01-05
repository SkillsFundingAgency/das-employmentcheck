using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber
{
    public class NationalInsuranceNumberYearsService : INationalInsuranceNumberYearsService
    {
        private readonly DataCollectionsApiConfiguration _apiConfiguration;
        private readonly IDataCollectionsApiClient<DataCollectionsApiConfiguration> _apiClient;
        private readonly IMemoryCache _memoryCache;
        const string CacheKey = "NationalInsuranceNumberYearsServiceKey";

        public NationalInsuranceNumberYearsService(
            IDataCollectionsApiClient<DataCollectionsApiConfiguration> apiClient,
            DataCollectionsApiConfiguration apiConfiguration,
            IMemoryCache memoryCache)
        {
            _apiClient = apiClient;
            _apiConfiguration = apiConfiguration;
            _memoryCache = memoryCache;
        }

        public Task<IEnumerable<string>> Get()
        {
            return _memoryCache.GetOrCreateAsync(CacheKey, (c) =>
            {
                c.AbsoluteExpiration = DateTime.UtcNow.AddSeconds(_apiConfiguration.AcademicYearsCacheDurationSecs);                
                return RefreshList();
            });
        }

        private async Task<IEnumerable<string>> RefreshList()
        {   
            var request = new GetNationalInsuranceNumberYearsRequest(_apiConfiguration);
            var response = await _apiClient.Get(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<string>();
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            List<string> years = new List<string>();

            try 
            {
                years = new List<string>(JsonConvert.DeserializeObject<string[]>(jsonString));

                if (years.Count > _apiConfiguration.NumberOfAcademicYearsToSearch)
                {
                    years = years.OrderByDescending(y => y).Take(_apiConfiguration.NumberOfAcademicYearsToSearch).ToList();
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Response string : {jsonString}, caused error :{ex.Message}");
            }

            return years.OrderByDescending(y => y);
        }
    }

}
