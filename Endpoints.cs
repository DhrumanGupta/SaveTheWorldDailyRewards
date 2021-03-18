namespace SaveTheWorldRewards
{
    class Endpoints
    {
        public static string ac = "https://www.epicgames.com/id/logout?redirectUrl=https%3A%2F%2Fwww.epicgames.com%2Fid%2Flogin%3FredirectUrl%3Dhttps%253A%252F%252Fwww.epicgames.com%252Fid%252Fapi%252Fredirect%253FclientId%253Dec684b8c687f479fadea3cb2ad83f5c6%2526responseType%253Dcode";
        public static string token = "https://account-public-service-prod.ol.epicgames.com/account/api/oauth/token";
        public static string Reward(string authToken)
        {
            return $"https://fortnite-public-service-prod11.ol.epicgames.com/fortnite/api/game/v2/profile/{authToken}/client/ClaimLoginReward?profileId=campaign";
        }
    }
}
