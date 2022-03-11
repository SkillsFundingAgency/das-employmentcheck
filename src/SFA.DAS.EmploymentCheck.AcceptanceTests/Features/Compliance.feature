Feature: Compliance
	End-to-end employment check happy path

Scenario: Employment Check rerurns positive result
	Given an unprocessed employment check
	And a valid PAYE scheme is returned for the Account
	And a valid National Insurance Number returned for the learner
	When the Employment Check is performed
	Then the Employment Check result is stored