Feature: DataCollectionNinoBadApiResponse
		What application should do when a certain response is returned when attempting to get a NI Number


Scenario: DataCollection Api returns unsuccessful status code when attempting to get NI Number
	Given an employment check cache request
	When DataCollection Api call returns <StatusCode> status code
	Then there is a <FailureCode> returned

Examples: 
 | StatusCode | FailureCode		|
 | 500        | "NinoFailure"	|
 | 400        | "NinoFailure"	|
 | 404        | "NinoFailure"	|
 | 200        | "NinoInvalid"	|
