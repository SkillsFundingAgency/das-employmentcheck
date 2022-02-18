﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Database;
using SFA.DAS.HashingService;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests
{
    public class TestContext : IDisposable
    {
        public Fixture Fixture;
        public string InstanceId { get; private set; }
        public DirectoryInfo TestDirectory { get; set; }
        public TestFunction TestFunction { get; set; }
        public SqlDatabase SqlDatabase { get; set; }
        public MockApi EmployerAccountsApi { get; set; }
        public MockApi DataCollectionsApi { get; set; }
        public MockApi HmrcApi { get; set; }
        public IHashingService HashingService { get; set; }
        public TestData TestData { get; set; }

        public TestContext()
        {
            Fixture = new Fixture();
            InstanceId = Guid.NewGuid().ToString();
            TestDirectory = new DirectoryInfo(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.Parent.FullName, $"TestDirectory/{InstanceId}"));
            if (!TestDirectory.Exists)
            {
                Directory.CreateDirectory(TestDirectory.FullName);
            }

            HashingService = new HashingService.HashingService("46789BCDFGHJKLMNPRSTVWXY", "test hash string");
            TestData = new TestData();
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