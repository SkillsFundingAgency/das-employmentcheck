using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.DataSeeder
{
    static class Program
    {
        public static async Task Main()
        {
            // Removed colour changes as red error text on a blue background was difficult to read
            Console.Clear();
            Console.WriteLine($"---------- DAS App Levy Data Seeder v1.0 (2022-01-07) ----------");
            Console.WriteLine();

            await new DataSeeder().DoTheWork();
        }
    }
}
