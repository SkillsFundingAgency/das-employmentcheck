using AutoFixture;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Commands;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Hooks;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.EmploymentCheck.Tests.Database;
using SFA.DAS.HashingService;
using System;
using System.Collections.Generic;
using System.IO;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests
{
    public class TestContext : IDisposable
    {
        public Fixture Fixture;
        public string InstanceId { get; }
        public DirectoryInfo TestDirectory { get; set; }
        public TestFunction TestFunction { get; set; }
        public SqlDatabase SqlDatabase { get; set; }
        public MockApi EmployerAccountsApi { get; set; }
        public MockApi DataCollectionsApi { get; set; }
        public DataCollectionsApiConfiguration DataCollectionsApiConfiguration { get; set; }
        public MockApi HmrcApi { get; set; }
        public IHashingService HashingService { get; set; }
        public TestData TestData { get; set; }
        public List<IHook> Hooks { get; set; }
        public List<object> EventsPublished { get; set; }
        public List<PublishedEvent> PublishedEvents { get; set; }
        public List<PublishedCommand> CommandsPublished { get; set; }
        public TestMessageBus MessageBus { get; set; }

        public TestContext()
        {
            Fixture = new Fixture();
            InstanceId = Guid.NewGuid().ToString();
            TestDirectory = new DirectoryInfo(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent?.Parent?.Parent?.FullName!, $"TestDirectory/{InstanceId}"));
            if (!TestDirectory.Exists)
            {
                Directory.CreateDirectory(TestDirectory.FullName);
            }
            TestData = new TestData();
            TestData.GetOrCreate("ThrowErrorAfterPublishCommand", () => false);
            TestData.GetOrCreate("ThrowErrorAfterProcessedCommand", () => false);
            TestData.GetOrCreate("ThrowErrorAfterPublishEvent", () => false);
            HashingService = new HashingService.HashingService("46789BCDFGHJKLMNPRSTVWXY", "test hash string");
            Hooks = new List<IHook>();
            EventsPublished = new List<object>();
            PublishedEvents = new List<PublishedEvent>();
            CommandsPublished = new List<PublishedCommand>();
            DataCollectionsApiConfiguration = new DataCollectionsApiConfiguration();
        }

        private bool _isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                EmployerAccountsApi?.Reset();
                DataCollectionsApi?.Reset();
            }

            _isDisposed = true;
        }
    }
}