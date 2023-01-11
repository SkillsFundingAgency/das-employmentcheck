Feature: EmployerAccountApiResponse
		What application should do when an unsuccessful response is returned from accounts api

Scenario: AccountsApiResponse Api returns unsuccessful status code
	Given an account pay schemes request
	When DataCollection Api call returns <StatusCode> status code 
	Then the Api call with <StatusCode> is retried <RetryCount> times
	And the error response and <StatusCode> are persisted

Examples: 
 | StatusCode | RetryCount |
 | 401        | 3          |
 | 500        | 3          |
 | 400        | 0          |
 | 404        | 0          |
