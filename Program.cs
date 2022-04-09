using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;

namespace SaveTheWorldRewards
{
	class Program
	{
        private static readonly string _path = @"lastUsed.txt";

        private static async Task Main()
        {
            if (!HasRunToday())
            {
                string authCode = await WebdriverManager.GetAuthCode();
                bool result = await HttpPostManager.Daily(authCode);

                if (result)
                {
                    string utcToday = (DateTime.UtcNow.DayOfYear == 365 || DateTime.UtcNow.DayOfYear == 366) ? DateTime.MinValue.ToString(CultureInfo.InvariantCulture) : DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

                    Console.WriteLine("Saving current date as " + utcToday);
                    File.WriteAllText(_path, utcToday);
                }
                
                Console.ReadKey();
            }
        }

        private static bool HasRunToday()
        {
            if (!File.Exists(_path))
            {
                Console.WriteLine("File does not exist, claiming reward...");
                return false;
            }

            Console.WriteLine();

            string s = File.ReadAllText(_path);
            Console.WriteLine($"Last claimed at {s}");
            var lastUsed = DateTime.Parse(s, CultureInfo.InvariantCulture);


            if (lastUsed.DayOfYear >= DateTime.UtcNow.DayOfYear)
            {
                Console.WriteLine("Reward Already Claimed!");
                return true;
            }
            return false;
        }
    }
}