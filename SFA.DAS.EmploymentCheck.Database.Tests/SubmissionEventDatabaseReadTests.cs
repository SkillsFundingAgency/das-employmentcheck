using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Orchestrators;
using SFA.DAS.EmploymentCheck.Database.Tests.Helpers;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using StructureMap;
using SubmissionEventWorkerRole.DependencyResolution;

namespace SFA.DAS.EmploymentCheck.Database.Tests
{
    [TestFixture]
    public class WhenReadingFromTheSubmissionEventsTable
    {
        private Container _container;
        private SubmissionEventOrchestrator _orchestrator;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            _container = new Container(c =>
            {
                c.AddRegistry<DefaultRegistry>();
            });

            var tables = new List<string>
            {
                {"[employer_check].[DAS_SubmissionEvents]"}
            };

            SqlHelpers.TruncateTables(tables, _container.GetInstance<IConfiguration>().DatabaseConnectionString).Wait();
        }

        [Test]
        public async Task ThenIfNoDataInTheTableTheLastKnownEventIdShouldBelessThanZero()
        {
            _orchestrator = new SubmissionEventOrchestrator(_container.GetInstance<IMediator>());

           var actual = await _orchestrator.GetLastKnownProcessedSubmissionEventId();

            Assert.AreEqual(-1, actual.Data);
        }
    }
}
