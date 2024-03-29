﻿using System;
using WireMock.Server;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests
{
    public class MockApi : IDisposable
    {
        private bool _isDisposed;

        public string BaseAddress { get; private set; }

        public WireMockServer MockServer { get; private set; }

        public MockApi()
        {
            MockServer = WireMockServer.Start();
            BaseAddress = MockServer.Urls[0];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            MockServer.Reset();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                if (MockServer.IsStarted)
                {
                    MockServer.Stop();
                }
                MockServer.Dispose();
            }

            _isDisposed = true;
        }
    }
}
