Feature: DataCollectionApiResponse
		What application should do when a certain response is returned

Scenario: DataCollection Api returns unsuccessful status code
	Given an employment check cache request
	When DataCollection Api call returns <StatusCode> status code 
	Then the Api call with <StatusCode> is retried <RetryCount> times

Examples: 
 | StatusCode | RetryCount |
 | 401        | 3          |
 | 500        | 3          |
 | 400        | 0          |
 | 404        | 0          |
