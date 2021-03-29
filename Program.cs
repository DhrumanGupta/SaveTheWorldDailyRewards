using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace SaveTheWorldRewards
{
	class Program
	{
        private static string path = @"lastUsed.txt";

        private static async Task Main()
        {
            if (!HasRunToday())
            {
                string authCode = WebdriverManager.GetAuthCode();
                //Console.WriteLine($"Using code: {authCode}");
                await Daily(authCode);
            }
        }

        private static bool HasRunToday()
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist, claiming reward...");
                return false;
            }

            Console.WriteLine();

            string s = File.ReadAllText(path);
            Console.WriteLine($"Last claimed at {s}");
            var lastUsed = DateTime.Parse(s);


            if (lastUsed.DayOfYear >= DateTime.UtcNow.DayOfYear)
            {
                Console.WriteLine("Reward Already Claimed!");
                return true;
            }
            return false;
        }

		private static (bool, string, string) GetToken(string authCode)
		{
			HttpClient client = new HttpClient();
			var d = new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code"},
                { "code", authCode }
            };

			var content = new FormUrlEncodedContent(d);
			client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "basic ZWM2ODRiOGM2ODdmNDc5ZmFkZWEzY2IyYWQ4M2Y1YzY6ZTFmMzFjMjExZjI4NDEzMTg2MjYyZDM3YTEzZmM4NGQ=");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

            var postR = client.PostAsync(Endpoints.token, content);
            while (!postR.IsCompleted) { }
			if (postR.Result.StatusCode != HttpStatusCode.OK)
            {
				return (false, postR.Result.ReasonPhrase, postR.Result.Content.ToString());
            }
			string s = postR.Result.Content.ReadAsStringAsync().Result;

			Response r;

			try
            {
				r = JsonConvert.DeserializeObject<Response>(s);
			}
			catch (Exception e)
            {
				Console.WriteLine(e);
				return (false, string.Empty, string.Empty);
            }
			

			var access_token = r.access_token;
			var account_id = r.account_id;
			return (true, access_token, account_id);
		}

		private static async Task Daily(string token)
		{
            if (token == null) return;
			if (token?.Length != 32)
            {
				Console.WriteLine("Please provide a valid token!");
            }
			else
            {
				(bool tokenIsValid, string s1, string s2) = GetToken(token);

				if (!tokenIsValid)
                {
					Console.WriteLine($"{s1}  {s2}");
                }
				else
                {
                    await RunPython(Endpoints.Reward(s2), s1);

                    string utcToday = (DateTime.UtcNow.DayOfYear == 365 || DateTime.UtcNow.DayOfYear == 366) ? new DateTime(0).ToString() : DateTime.UtcNow.ToString();

                    Console.WriteLine();
                    Console.WriteLine("Saving current date as " + utcToday);

                    using (StreamWriter file = new StreamWriter(path))
                    {
                        file.WriteLine(utcToday);
                    }

                    Console.ReadKey();
                }
            }
		}

        private static string GetPythonPath(string requiredVersion = "", string maxVersion = "") {
            string[] possiblePythonLocations = new string[3] {
                @"HKLM\SOFTWARE\Python\PythonCore\",
                @"HKCU\SOFTWARE\Python\PythonCore\",
                @"HKLM\SOFTWARE\Wow6432Node\Python\PythonCore\"
            };

            //Version number, install path
            Dictionary<string, string> pythonLocations = new Dictionary<string, string>();

            foreach (string possibleLocation in possiblePythonLocations) {
                string regKey = possibleLocation.Substring(0, 4), actualPath = possibleLocation.Substring(5);
                RegistryKey theKey = (regKey == "HKLM" ? Registry.LocalMachine : Registry.CurrentUser);
                RegistryKey theValue = theKey.OpenSubKey(actualPath);

                foreach (var v in theValue.GetSubKeyNames()) {
                    RegistryKey productKey = theValue.OpenSubKey(v);
                    if (productKey != null) {
                        try {
                            string pythonExePath = productKey.OpenSubKey("InstallPath").GetValue("ExecutablePath").ToString();
                            if (pythonExePath != null && pythonExePath != "") {
                                //Console.WriteLine("Got python version; " + v + " at path; " + pythonExePath);
                                pythonLocations.Add(v.ToString(), pythonExePath);
                            }
                        } catch {
                            //Install path doesn't exist
                        }
                    }
                }
            }

            if (pythonLocations.Count > 0) {
                System.Version desiredVersion = new System.Version(requiredVersion == "" ? "0.0.1" : requiredVersion),
                    maxPVersion = new System.Version(maxVersion == "" ? "999.999.999" : maxVersion);

                string highestVersion = "", highestVersionPath = "";

                foreach (KeyValuePair<string, string> pVersion in pythonLocations) {
                    //TODO; if on 64-bit machine, prefer the 64 bit version over 32 and vice versa
                    int index = pVersion.Key.IndexOf("-"); //For x-32 and x-64 in version numbers
                    string formattedVersion = index > 0 ? pVersion.Key.Substring(0, index) : pVersion.Key;

                    System.Version thisVersion = new System.Version(formattedVersion);
                    int comparison = desiredVersion.CompareTo(thisVersion),
                        maxComparison = maxPVersion.CompareTo(thisVersion);

                    if (comparison <= 0) {
                        //Version is greater or equal
                        if (maxComparison >= 0) {
                            desiredVersion = thisVersion;

                            highestVersion = pVersion.Key;
                            highestVersionPath = pVersion.Value;
                        } else {
                            //Console.WriteLine("Version is too high; " + maxComparison.ToString());
                        }
                    } else {
                        //Console.WriteLine("Version (" + pVersion.Key + ") is not within the spectrum.");
                    }
                }

                //Console.WriteLine(highestVersion);
                //Console.WriteLine(highestVersionPath);
                return highestVersionPath;
            }

            return "";
        }
		private static async Task RunPython(string url, string authCode)
        {
            // 1) Create Process Info
            var psi = new ProcessStartInfo
            {
                FileName = @GetPythonPath()
            };

            // 2) Provide script and arguments
            var script = @"stwRewards.py";

            psi.Arguments = $"\"{script}\" \"{url}\" \"{authCode}\"";

            // 3) Process configuration
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            // 4) Execute process and get output
            var errors = "";
            var results = "";

            using (var process = Process.Start(psi))
            {
                errors = await process.StandardError.ReadToEndAsync();
                results = await process.StandardOutput.ReadToEndAsync();
            }

            // 5) Display output
            Console.WriteLine(errors);
            Console.WriteLine();
            Console.WriteLine(results);
        }
	}
}
