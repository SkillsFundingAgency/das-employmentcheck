﻿Feature: Compliance
		In order to identify learners that are being paid
		As the SFA Funding Operational team
		I want to extract information from various system
		So that SFA Monitoring Team can carry on with further investigation.

@AMl1297
Scenario Outline: Employment Check
Given A Submission Event has raised with Apprenticeship <ApprenticeshipId> and NINO <Nino> and ULN <Uln> and Ukprn <Ukprn>
And a Commitment with Apprenticeship <ApprenticeshipId> and Ukprn <Ukprn> and Account Id <AccountId> exists
And An Account with an Account Id <AccountId> and EmpRef <EmpRefs> exists
And Hmrc Api is configured as 
| Paye        | Nino   | Response       |
| 444/AA00001 | <Nino> | NotEmployed    |
| 555/AA00001 | <Nino> | NotEmployed    |
| 333/AA00001 | <Nino> | <Hmrcresponse> |
When I run the worker role
Then I should have PassedValidationCheck <Check> for ULN <Uln> and NINO <Nino>

Examples: 
| AccountId | EmpRef      | Nino      | Uln        | Hmrcresponse | Check | Ukprn    | ApprenticeshipId | EmpRefs                             |
| 24979     | 333/AA00001 | QQ123456C | 5641235789 | Employed     | Yes   | 10007898 | 112233           | 444/AA00001,555/AA00001,333/AA00001 |
| 24979     | 333/AA00001 | QQ123456D | 5641235779 | NotEmployed  | No    | 10007898 | 112234           | 333/AA00001,555/AA00001             |



@AML2073
Scenario Outline: UKPRN mismatch
Given A Submission Event has raised with Apprenticeship <ApprenticeshipId> and NINO <Nino> and ULN <Uln> and Ukprn <Ukprn>
And a Commitment with Apprenticeship <ApprenticeshipId> and Ukprn <Ukprn> and Account Id <AccountId> throws 401
And An Account with an Account Id <AccountId> and EmpRef <EmpRef> exists
And Hmrc Api is configured as 
| Paye     | Nino   | Response |
| <EmpRef> | <Nino> | Employed |
When I run the worker role
Then I should have PassedValidationCheck <Check> for ULN <Uln> and NINO <Nino>

Examples: 
| AccountId | EmpRef      | Nino      | Uln        | Check | Ukprn    | ApprenticeshipId |
| 55443     | 333/AA00001 | TT123456D | 5641235779 | No    | 10007898 | 112234           |


@AML2073
Scenario Outline: Hmrc returned Not Found
Given A Submission Event has raised with Apprenticeship <ApprenticeshipId> and NINO <Nino> and ULN <Uln> and Ukprn <Ukprn>
And a Commitment with Apprenticeship <ApprenticeshipId> and Ukprn <Ukprn> and Account Id <AccountId> exists
And An Account with an Account Id <AccountId> and EmpRef <EmpRef> exists
And Hmrc Api is configured to return Not Found for  
| Paye     | Nino   | Response       |
| <EmpRef> | <Nino> | <Hmrcresponse> |
When I run the worker role
Then I should have PassedValidationCheck <Check> for ULN <Uln> and NINO <Nino>

Examples: 
| AccountId | EmpRef      | Nino      | Uln        | Hmrcresponse | Check | Ukprn    | ApprenticeshipId |
| 24978     | 333/AA00001 | QQ123456D | 5641235779 | NotEmployed  | No    | 10007898 | 112234           |


@AMl2233
@addmoreEmpRefstoanAccount
Scenario Outline: Azure message lock should be automatically renewed
Given A Submission Event has raised with Apprenticeship <ApprenticeshipId> and NINO <Nino> and ULN <Uln> and Ukprn <Ukprn>
And a Commitment with Apprenticeship <ApprenticeshipId> and Ukprn <Ukprn> and Account Id <AccountId> exists
And An Account with an Account Id <AccountId> and EmpRef <EmpRefs> exists
And Hmrc Api is configured as 
| Paye         | Nino   | Response       |
| 9997/AA00001 | <Nino> | NotEmployed    |
| 9998/AA00001 | <Nino> | NotEmployed    |
| 9999/AA00001 | <Nino> | <Hmrcresponse> |
When I run the worker role
Then I should have PassedValidationCheck <Check> for ULN <Uln> and NINO <Nino>

Examples: 
| AccountId | EmpRef      | Nino      | Uln        | Hmrcresponse | Check | Ukprn    | ApprenticeshipId | EmpRefs                                |
| 22330     | 333/AA00001 | AZ123456C | 5641235789 | Employed     | Yes   | 10007898 | 112233           | 9997/AA00001,9998/AA00001,9999/AA00001 |

