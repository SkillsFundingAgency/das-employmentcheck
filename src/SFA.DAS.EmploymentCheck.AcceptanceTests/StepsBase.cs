using AutoFixture;
using FluentAssertions;
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
        }
    }
}
