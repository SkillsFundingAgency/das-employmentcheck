Feature: Compliance
	In order to identify learners 
	As a math idiot
	I want to be told the sum of two numbers

@AMl1297
Scenario Outline: Employment Check
Given A Submission Event has raised with Apprenticeship <ApprenticeshipId> and NINO <Nino> and ULN <Uln> and Ukprn <Ukprn>
And a Commitment with Apprenticeship <ApprenticeshipId> and Ukprn <Ukprn> and Account Id <AccountId> exists
And An Account with an Account Id <AccountId> and EmpRef <EmpRef> exists
And a call to the HMRC API with EmpRef <EmpRef> and NINO <Nino> response <Hmrcresponse>
When I run the worker role
Then I should have PassedValidationCheck <Check> for ULN <Uln> and NINO <Nino>

Examples: 
| AccountId | EmpRef      | Nino      | Uln        | Hmrcresponse | Check | Ukprn    | ApprenticeshipId |
| 24979     | 333/AA00001 | QQ123456C | 5641235789 | Employed     | Yes   | 10007898 | 112233           |
| 24979     | 333/AA00001 | QQ123456D | 5641235779 | NotEmployed  | No    | 10007898 | 112234           |