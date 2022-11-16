Feature: EmployerAccountPAYEBadApiResponse
	What application should do when a certain response is returned when attempting to get PAYE Info


Scenario: EmployerAccount Api returns unsuccessful status code when attempting to get PAYE Info
	Given an employment check cache request
	When EmployerAccount Api call returns <StatusCode> status code
	Then there is a <FailureCode> returned

Examples: 
 | StatusCode | FailureCode		|
 | 500        | "PAYEFailure"	|
 | 400        | "PAYEFailure"	|
 | 404        | "PAYENotFound"	|
