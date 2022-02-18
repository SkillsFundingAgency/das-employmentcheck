Feature: Compliance
	End-to-end employment check happy path

Scenario: Happy Path Test
	Given an unprocessed employment check for AccountId 123 and ULN number 12345678
	And a valid PAYE scheme is returned for the Account
	And a valid National Insurance Number returned for the learner
	When the Employment Check is performed
	Then the Employment Check result is stored