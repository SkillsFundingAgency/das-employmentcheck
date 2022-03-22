using System;
using System.Collections.Concurrent;
using AutoFixture;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests
{
    public class TestData
    {
        private readonly ConcurrentDictionary<string, object> _testdata = new ConcurrentDictionary<string, object>();
        private readonly Fixture _fixture = new Fixture();

        public T GetOrCreate<T>(string key = null, Func<T> onCreate = null)
        {
            key ??= typeof(T).FullName;

            _testdata.TryAdd(key, onCreate == null ? _fixture.Create<T>() : onCreate.Invoke());

            return (T)_testdata[key];
        }

        public T Get<T>(string key = null)
        {
            if (key == null)
            {
                key = typeof(T).FullName;
            }

            return (T)_testdata[key];
        }

        public void Set<T>(string key, T value)
        {
            _testdata[key] = value;
        }
    }
}
