Feature: PrioritiseChecks
	Employer Incentives team will send to the Employment Check solution the learners that require checking. 
	The learners will be placed on a queuing system (database) and checked on a first in first out basis.
	When an employer has multiple PAYE Schemes in their organisation, the Employment Check solution will check 
	them systematically until the learner is found. This may cause delays if there are many PAYE schemes.

Scenario: Employment Checks prioritise smaller employers
	Given a number of New Pending employment checks are added
	And  a number of Previously processed employment checks
	And valid National Insurance Numbers are returned for the learners
	And valid PAYE schemes are returned for the Accounts
	When the Employment Checks are performed
	Then the Employment Checks are performed in order of PayeScheme Priority Order
	And the Employment Checks Sent to Hmrc order of PayeScheme Priority Order