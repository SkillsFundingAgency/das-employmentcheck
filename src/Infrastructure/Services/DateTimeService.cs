using SFA.DAS.EmploymentCheck.Application.Common.Interfaces;
using System;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.Now;
    }
}
