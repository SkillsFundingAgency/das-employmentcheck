Feature: HmrcApiCallRetryTime
	When application needs to retry an Hmrc Api call it should be done within a certain time span

Scenario: Hmrc Api returns status code where retry is required
	Given an existing employment check cache request
	When Hmrc Api call returns 500 status code
	Then then the Api has done 3 retries within 30 seconds

