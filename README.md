[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/das-employmentcheck?repoName=SkillsFundingAgency%2Fdas-employmentcheck&branchName=refs%2Fpull%2F159%2Fmerge)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=2539&repoName=SkillsFundingAgency%2Fdas-employmentcheck&branchName=refs%2Fpull%2F159%2Fmerge)

# Employment Check
This repository represents the Employment Check code base. The idea around this check is to allow the business to understand which learners have been validated to show that they are employed by the employer who is associated with them on the service. 

Any learners who are not employed by the correct employer should not be on an apprenticeship with that employer and it is a vector which is obviously fraudulent. 

## Developer setup - Functions App

### Requirements

In order to run this solution locally you will need the following:

* [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
* [.NET Core SDK >= 3.1](https://www.microsoft.com/net/download/)
* (VS Code Only) [C# Extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
* [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
* [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (previosuly known as Azure Storage Emulator)

### Environment Setup
* Deploy local database from the `SFA.DAS.EmploymentCheck.Database` Visual Studio project
* Add the following configuration in a `local.settings.json` file for `SFA.DAS.EmploymentCheck.Functions` project:
```
{
  "IsEncrypted": false,
  "AzureWebJobsStorage": "UseDevelopmentStorage=true",
  "Values": {
    "EnvironmentName": "LOCAL",
    "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ConfigNames": "SFA.DAS.EmploymentCheck.Functions",
    "Version": "1.0",
    "ResponseOrchestratorTriggerTime": "* 0 * * * *",
    "EmploymentCheckTriggerTime": "0 0/15 0 * * *"
  },
  "ApplicationSettings": {
    "DbConnectionString": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SFA.DAS.EmploymentCheck.Database;Integrated Security=True;Pooling=False;Connect Timeout=30;MultipleActiveResultSets=True",
    "AllowedHashstringCharacters": "<INSERT_HERE>",
    "Hashstring": "<INSERT_HERE>",
    "NServiceBusConnectionString": "UseLearningEndpoint=true",
    "NServiceBusLicense": "C:\\<INSERT_HERE>\\License.xml"
  },
  "AccountsInnerApi": {
    "url": "https://<INSERT_HERE>/",
    "identifier": "https://<INSERT_HERE>"
  },
  "HmrcApiSettings": {
    "BaseUrl": "https://<INSERT_HERE>/"
  },
  "HmrcAuthTokenService": {
    "TokenUrl": "https://<INSERT_HERE>",
    "ClientId": "<INSERT_HERE>",
    "ClientSecret": "<INSERT_HERE>",
    "TotpSecret": "<INSERT_HERE>"
  },
  "DcApiSettings": {
    "BaseUrl": "https://<INSERT_HERE>",
    "Tenant": "<INSERT_HERE>",
    "IdentifierUri": "<INSERT_HERE>",
    "ClientId": "<INSERT_HERE>",
    "ClientSecret": "<INSERT_HERE>"
  }
}
```

### Running

* Start Azurite e.g. using a command `C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator>AzureStorageEmulator.exe start`
* Open command prompt and change directory to `src\SFA.DAS.EmploymentCheck.Functions\`
* Launch the Azure functions runtime host using command `func start`

### Application logs
Application logs for are for local environment are written to text files stored in the `src\SFA.DAS.EmploymentCheck.Functions\bin\output\logs\` folder.

## Developer setup - API

### Requirements

In order to run this solution locally you will need the following:

* [.NET Core SDK >= 3.1](https://www.microsoft.com/net/download/)
* (VS Code Only) [C# Extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
* [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

### Environment Setup
* Deploy local database from the `SFA.DAS.EmploymentCheck.Database` Visual Studio project
* Add the following configuration in a `appsettings.Development.json` file for `SFA.DAS.EmploymentCheck.Api` project:
```
{
  "ConfigNames": "SFA.DAS.EmploymentCheck",
  "EnvironmentName": "LOCAL",
  "DbConnectionString": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SFA.DAS.EmploymentCheck.Database;Integrated Security=True;Pooling=False;Connect Timeout=30;MultipleActiveResultSets=True"
}
```


### Running
* Open command prompt and change directory to `src\SFA.DAS.EmploymentCheck.Api\`
* Launch the Azure functions runtime host using command `dotnet run`
* Open any browser and navigate to `https://localhost:5001/swagger/index.html` to access Swagger API User Interface page

### Application logs
Application logs for are for local environment are written to text files stored in the `src\SFA.DAS.EmploymentCheck.Api\logs\` folder.

## Tools and Utilities
### HMRC API Authentication Service stub
As there currently is no hosted test system for authenticating calls to HMRC API, we have developed a stub, which is being used in all non-production environments. The code is contained in the `SFA.DAS.EmploymentCheck.TokenServiceStub` project and its configuration is the `HmrcAuthTokenService` section of the main Function App settings.

### Test Harness
Contained in `SFA.DAS.EmploymentCheck.TestHarness` project, test harness is an API with Swagger UI for making single employment check calls to non-production HMRC API environments. It uses HMRC API Authentication Service stub for authentication and has two endpoints:
* `GET /EmploymentCheckHttpTestHarness/token-test/` - retrieves authentication token from the stub and makes a call to HMRC API with hard-coded employment check inputs
* `GET /EmploymentCheckHttpTestHarness/` - retrieves authentication token from the stub and makes a call to HMRC API with passed-in employment check inputs:
    * **nino** (string) - Employee's National Insurance Number
    * **empRef** (string) - Employer's PAYE scheme
    * **fromDate** (datetime) - Employment start date
    * **toDate** (datetime) - Employment end date

#### Sample request
`https://localhost:5001/EmploymentCheckHttpTestHarness?nino=PR555555A&empRef=923%2FEZ00059&fromDate=2010-01-01&toDate=2018-01-01`
#### Sample response
```csharp
{
  "empref": "923/EZ00059",
  "nino": "PR555555A",
  "fromDate": "2010-01-01T00:00:00.000Z",
  "toDate": "2018-01-01T00:00:00.000Z",
  "employed": true
}
```

#### Configuration

### Data Seeder
This console application allows seeding data from a CSV file into Employment Checks database. It supports  connections to local SQL server instance as well Azure SQL databases, including all Employment Check test environments.
The sample data file from the `Files` folder in `SFA.DAS.EmploymentCheck.DataSeeder` project contains 25 rows of data, some of which will return positive results from Accounts, Data Collections and HMRC API's test environments. Data from this file is seeded into the main checks input database table `Business.EmploymentCheck` and has an option to also seed the "cache request" table to limit the processing to just the HMRC API calls, bypassing the requests to Accounts and DC API.

#### CSV File inputs
* ULN
* AccountId
* fromDate
* toDate
* Nino

#### Settings
* **EmploymentChecksConnectionString** (string) - target database connection string
* **DataSets** (int) - number of data sets to seed, i.e. if it's set to 3, each row from the input data file will be inserted 3 times 
* **ClearExistingData** (boolean) - if set to *true*, all data in the following tables will be cleared:
    * [Business].[EmploymentCheck]
    * [Cache].[AccountsResponse]
    * [Cache].[EmploymentCheckCacheRequest]
    * [Cache].[EmploymentCheckCacheResponse]
    * [Cache].[DataCollectionsResponse]

* **SeedEmploymentCheckCacheRequests** (boolean) - if set to *true*, then in addition to seeding the `EmploymentCheck` table, the `Cache.EmploymentCheckCacheRequest` will also be seeded with data thus bypassing the calls to Accounts and DC API when processing. This option is good for HMRC API performance tests.

## License
Licensed under the [MIT license](LICENSE)
