Feature: PrioritiseChecks
	Employer Incentives team will send to the Employment Check solution the learners that require checking. 
	The learners will be placed on a queuing system (database) and checked on a first in first out basis.
	When an employer has multiple PAYE Schemes in their organisation, the Employment Check solution will check 
	them systematically until the learner is found. This may cause delays if there are many PAYE schemes.

Scenario: Employment Checks are prioritise smaller employers
	Given a number of unprocessed employment checks
	And valid PAYE schemes are returned for the Accounts
	And valid National Insurance Numbers are returned for the learners
	When the Employment Checks are performed
	Then the Employment Checks are performed in order of smallest to largest employers