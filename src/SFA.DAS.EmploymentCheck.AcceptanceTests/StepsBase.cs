using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Commands;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Hooks;
using System;
using System.Linq;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests
{
    public class StepsBase
    {
        protected readonly TestContext TestContext;
        protected readonly Fixture Fixture;
        private static readonly object Lock = new object();

        public StepsBase(TestContext testContext)
        {
            TestContext = testContext;
            Fixture = new Fixture();

            AssertionOptions.AssertEquivalencyUsing(options =>
            {
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>();
                options.Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTimeOffset>();
                return options;
            });

            if (testContext.Hooks.SingleOrDefault(h => h is Hook<object>) is Hook<object> hook)
            {
                hook.OnReceived += (message) =>
                {
                    lock (Lock)
                    {
                        testContext.PublishedEvents.Add(
                            new PublishedEvent(message)
                            {
                                IsReceived = true
                            });
                    }
                };

                hook.OnProcessed += (message) =>
                {
                    lock (Lock)
                    {
                        testContext.PublishedEvents.Add(
                            new PublishedEvent(message)
                            {
                                IsProcessed = true
                            });

                        testContext.EventsPublished.Add(message);
                        var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterPublishEvent");
                        if (throwError)
                        {
                            throw new ApplicationException("Unexpected exception, should force a rollback");
                        }
                    }
                };
            }

            if (!(testContext.Hooks.SingleOrDefault(h => h is Hook<ICommand>) is Hook<ICommand> commandsHook)) return;
            {
                commandsHook.OnReceived += (command) =>
                {
                    lock (Lock)
                    {
                        testContext.CommandsPublished.Add(
                            new PublishedCommand(command)
                            {
                                IsReceived = true,
                                IsDomainCommand = command is DomainCommand
                            });
                    }
                };

                commandsHook.OnDelayed += (command) =>
                {
                    lock (Lock)
                    {
                        testContext.CommandsPublished.Where(c => c.Command == command && c.IsDomainCommand == command is DomainCommand).ToList().ForEach(c => c.IsDelayed = true);
                    }
                };

                commandsHook.OnProcessed += (command) =>
                {
                    lock (Lock)
                    {
                        testContext.CommandsPublished.Where(c => c.Command == command && c.IsDomainCommand == command is DomainCommand).ToList().ForEach(c => c.IsProcessed = true);

                        var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterProcessedCommand");
                        if (throwError)
                        {
                            throw new ApplicationException("Unexpected exception, should force a rollback");
                        }
                    }
                };

                commandsHook.OnHandled += (command) =>
                {
                    lock (Lock)
                    {
                        var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterProcessedCommand");
                        if (throwError)
                        {
                            throw new ApplicationException("Unexpected exception, should force a rollback");
                        }
                    }
                };
                commandsHook.OnPublished += (command) =>
                {
                    lock (Lock)
                    {
                        testContext.CommandsPublished.Where(c => c.Command == command && c.IsDomainCommand == command is DomainCommand).ToList().ForEach(c => c.IsPublished = true);

                        var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterPublishCommand");
                        if (throwError)
                        {
                            throw new ApplicationException("Unexpected exception, should force a rollback");
                        }
                    }
                };

                commandsHook.OnErrored += (ex, command) =>
                {
                    lock (Lock)
                    {
                        var publishedCommands = testContext.CommandsPublished.Where(c => c.Command == command && c.IsDomainCommand == command is DomainCommand).ToList();

                        publishedCommands.ForEach(c =>
                        {
                            c.IsErrored = true;
                            c.LastError = ex;
                            if (ex.Message.Equals($"No destination specified for message: {command.GetType().FullName}"))
                            {
                                c.IsPublishedWithNoListener = true;
                            }
                        });

                        return ex.Message.Equals($"No destination specified for message: {command.GetType().FullName}");
                    }
                };
            }
        }
    }
}
