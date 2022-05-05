Feature: HmrcApiCallRetry
	HMRC Api call is retried on failure

Scenario: HMRC API returns unsuccessful status code
	Given an existing employment check cache request
	When Hmrc Api call returns <StatusCode> status code 
	Then the Api call with <StatusCode> is retried <RetryCount> times

Examples: 
 | StatusCode | RetryCount |
 | 401        | 3          |
 | 400        | 0          |
