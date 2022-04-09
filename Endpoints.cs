namespace SaveTheWorldRewards
{
    class Endpoints
    {
        public static string ac = "https://www.epicgames.com/id/api/redirect?clientId=ec684b8c687f479fadea3cb2ad83f5c6&responseType=code";
        public static string token = "https://account-public-service-prod.ol.epicgames.com/account/api/oauth/token";
        public static string Reward(string authToken)
        {
            return $"https://fortnite-public-service-prod11.ol.epicgames.com/fortnite/api/game/v2/profile/{authToken}/client/ClaimLoginReward?profileId=campaign";
        }
    }
}
