using System;
using System.Threading.Tasks;

namespace app_levy_data_seeder
{
    public class Program
    {
        private static async Task Main()
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"---------- DAS App Levy Data Seeder v0.2 (2021-12-08) ----------");
            Console.WriteLine();

            await new DataSeeder().DoTheWork();
        }
    }
}
