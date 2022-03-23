@messageBus
Feature: OutputApi
	Completed Employment Check is published onto the message bus

Scenario: Completed Employment Check is published onto the message bus
	Given Completed employment check
	When the output orchestrator is called
	Then the message with completed employment check is published
