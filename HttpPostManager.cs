using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SaveTheWorldRewards
{
    public static class HttpPostManager
    {
        public static async Task<bool> Daily(string token)
        {
            if (token == null) { return false; }

            if (token.Length != 32)
            {
                Console.WriteLine($"Try logging in again: {Endpoints.ac}");
                return false;
            }

            (bool tokenIsValid, string s1, string s2) = GetToken(token);

            if (!tokenIsValid)
            {
                Console.WriteLine($"{s1}  {s2}");
                return false;
            }

            string result = await GetResult(Endpoints.Reward(s2), s1);
            Console.WriteLine(result);

            return true;
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
                r = JsonSerializer.Deserialize<Response>(s);
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

        private static async Task<string> GetResult(string url, string authCode)
        {
            HttpResponseMessage postR = await MakeAuthPost(url, authCode);

            if (postR.StatusCode != HttpStatusCode.OK)
            {
                return postR.Content.ToString();
            }

            string s = postR.Content.ReadAsStringAsync().Result;;
            ParseFromJson(s, out JsonElement day, out string reward, out JsonElement items);

            if (items.GetArrayLength() <= 0)
            {
                return "Reward already claimed!";
            }

            string toShow = $"On day {day}, you received: {reward}";
 
            var nFounderItem = items[0].GetProperty("quantity").ToString();
            var founderItemJson = items[0].GetProperty("itemType").ToString();
            string founderItem = "";

            switch (founderItemJson)
            {
                case "CardPack:cardpack_event_founders":
                    founderItem = "Founder's Llama";
                    break;
                case "CardPack:cardpack_bronze":
                    founderItem = "Upgrade Llama (bronze)";
                    break;
            }

            toShow += $"\nFounders rewards: {nFounderItem} {founderItem}";
            return toShow;
        }

        private static void ParseFromJson(string s, out JsonElement day, out string reward, out JsonElement items)
        {
            JsonDocument responsetemp = JsonDocument.Parse(s);
            JsonElement response = responsetemp.RootElement;
            var notifications = response.GetProperty("notifications")[0];
            day = notifications.GetProperty("daysLoggedIn");

            int number = int.Parse(notifications.GetProperty("daysLoggedIn").ToString()) % 336;
            if (number == 0)
            {
                number = 1;
            }
            
            reward = Items.items[number.ToString()];
            items = notifications.GetProperty("items");
        }

        private static async Task<HttpResponseMessage> MakeAuthPost(string url, string authCode)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "bearer " + authCode);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            HttpContent content = new StringContent("{}", Encoding.UTF8, "application/json");
            var postR = await client.PostAsync(url, content);
            return postR;
        }
    }
}
