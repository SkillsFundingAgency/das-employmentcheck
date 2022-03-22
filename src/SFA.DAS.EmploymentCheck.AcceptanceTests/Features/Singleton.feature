Feature: Singleton
	Allow only a single instance of the employment check orchestrator

Scenario: A second instance of Employmnent Check orchestrator is triggered
	Given a running instance of Employment Check orchestrator
	When the orchestrator is triggered
	Then an error response is returned