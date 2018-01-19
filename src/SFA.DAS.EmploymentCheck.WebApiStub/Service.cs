using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.WebApiStub
{
    public class Service : IDictionary<int, List<string>>
    {
        public List<string> this[int key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICollection<int> Keys => throw new NotImplementedException();

        public ICollection<List<string>> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(int key, List<string> value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<int, List<string>> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<int, List<string>> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(int key)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<int, List<string>>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<int, List<string>>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(int key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<int, List<string>> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(int key, out List<string> value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
