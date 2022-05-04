Feature: DataCollectionApiResponse
		What application should do when a certain response is returned

@tag1
Scenario: DataCollection Api returns unsuccessful status code
	Given an existing employment check cache request
	When DataCollection Api call returns <StatusCode> status code 
	Then the Api call with <StatusCode> is retried <RetryCount> times

Examples: 
 | StatusCode | RetryCount |
 | 401        | 3          |
 | 400        | 0          |

