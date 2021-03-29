using System;
using System.Collections.Generic;
using System.Text;

namespace SaveTheWorldRewards
{
    class Response
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string expires_at { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public int refresh_expires { get; set; }
        public string refresh_expires_at { get; set; }
        public string account_id { get; set; }
        public string client_id { get; set; }
        public bool internal_client { get; set; }
        public string client_service { get; set; }
        public string[] scope { get; set; }
        public string displayName { get; set; }
        public string app { get; set; }
        public string in_app_id { get; set; }
        public string device_id { get; set; }
    }
}
