Feature: EmployementCheck



Scenario Outline: Employment Check
Given An Account with an Account Id <AccountId> and EmpRef <EmpRef> exists
And a call to the HMRC API with EmpRef <EmpRef> and NINO <Nino> response <Hmrcresponse>
When A Submission Event has raised with EmpRef <EmpRef> and NINO <Nino> and ULN <Uln>
Then I should have PassedValidationCheck <Check> for ULN <Uln> and NINO <Nino>

Examples: 
| AccountId | EmpRef      | Nino      | Uln        | Hmrcresponse | Check |
| 24979     | 333/AA00001 | QQ123456C | 5641235789 | Employed     | Yes   |
| 24979     | 333/AA00001 | QQ123456D | 5641235779 | NotEmployed  | No    |
