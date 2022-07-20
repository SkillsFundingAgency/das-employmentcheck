Feature: EmployerAccountApiRetryTime
	When application needs to retry it should be done within a certain time span


Scenario: Accounts Api returns status code where retry is required
	Given an employment check cache request
	When Accoints Api call returns 500 status code
	Then then the Api has done 3 retries within 30 seconds
