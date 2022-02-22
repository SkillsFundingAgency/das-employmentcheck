using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.DataSeeder
{
    static class Program
    {
        public static async Task Main()
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"---------- DAS App Levy Data Seeder v1.0 (2022-01-07) ----------");
            Console.WriteLine();

            await new DataSeeder().DoTheWork();
        }
    }
}
