using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Domain
{
    public class PageOfResults<T>
    {
        public int PageNumber { get; set; }
        public int TotalNumberOfPages { get; set; }
        public T[] Items { get; set; }
    }
}
