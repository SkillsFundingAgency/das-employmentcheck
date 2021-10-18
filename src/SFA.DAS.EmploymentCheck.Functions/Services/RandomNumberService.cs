using System;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public class RandomNumberService : IRandomNumberService
    {
        private static Random random = new Random();

        public bool GetRandomBool()
        {
            return Convert.ToBoolean(random.Next(0, 1));
        }
    }
}
