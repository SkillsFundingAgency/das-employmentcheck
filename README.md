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

## License

Licensed under the [MIT license](LICENSE)
