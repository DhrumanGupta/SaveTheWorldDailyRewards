using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
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
                    await getResult(Endpoints.Reward(s2), s1);

                    string utcToday = (DateTime.UtcNow.DayOfYear == 365 || DateTime.UtcNow.DayOfYear == 366) ? new DateTime(0).ToString() : DateTime.UtcNow.ToString();

                    Console.WriteLine();
                    Console.WriteLine("Saving current date as " + utcToday);

                    using (StreamWriter file = new StreamWriter(path))
                    {
                        file.WriteLine(utcToday);
                    }
                }
            }
		}
        public static async Task getResult(string url, string authCode)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "bearer " + authCode);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            HttpContent content = new StringContent("{}", Encoding.UTF8, "application/json");
            var postR = client.PostAsync(url, content);
            while (!postR.IsCompleted) { }
            if (postR.Result.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine(postR.Result.Content.ToString());
                Environment.Exit(59); //An unexpected network error occurred.
            }
            string s = postR.Result.Content.ReadAsStringAsync().Result;
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(s);
            var notifications = JsonConvert.DeserializeObject<Dictionary<string, string>>(response["notifications"]);
            var day = notifications["daysLoggedIn"];
            var reward = Items.items[notifications["daysLoggedIn"]];
            var items = JsonConvert.DeserializeObject<List<String>>(notifications["items"]);
            Console.WriteLine("On day " + day + ", you received: " + reward);
            if (items.Count > 1)
            {
                var fndr_amt = JsonConvert.DeserializeObject<Dictionary<string, string>>(items[2])["quantity"];
                var fndr_item_json = JsonConvert.DeserializeObject<Dictionary<string, string>>(items[2])["itemType"];
                var fndr_item = fndr_item_json;
                if (fndr_item_json == "CardPack:cardpack_event_founders")
                {
                    fndr_item = "Founder's Llama";
                }
                else if (fndr_item_json == "CardPack:cardpack_bronze")
                {
                    fndr_item = "Upgrade Llama (bronze)";
                }
                Console.WriteLine("Founders rewards: " + fndr_amt + " " + fndr_item);
            }
        }
    }
}