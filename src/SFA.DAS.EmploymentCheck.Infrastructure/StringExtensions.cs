using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Infrastructure
{
    public static class StringExtensions
    {
        public static Uri ToUri(this string theUrl)
        {
            return new Uri(theUrl);
        }
    }
}
