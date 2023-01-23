Feature: HmrcApiCallRetry
	HMRC Api call is retried on failure

Scenario: HMRC API returns unsuccessful status code
	Given an existing employment check cache request
	When Hmrc Api call returns <StatusCode> status code 
	Then the Api call with <StatusCode> is retried <RetryCount> times
	And the error response is persisted

Examples: 
 | StatusCode | RetryCount |
 | 401        | 3          |
 | 408        | 3          |
 | 500        | 3          |
 | 503        | 3          |
 | 400        | 0          |
 | 404        | 0          |
 | 403        | 0          |
 | 429        | 10         |
