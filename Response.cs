using System;
using System.Collections.Generic;
using System.Text;

namespace SaveTheWorldRewards
{
    class Response
    {
        public string access_token;
        public int expires_in;
        public string expires_at;
        public string token_type;
        public string refresh_token;
        public int refresh_expires;
        public string refresh_expires_at;
        public string account_id;
        public string client_id;
        public bool internal_client;
        public string client_service;
        public string[] scope;
        public string displayName;
        public string app;
        public string in_app_id;
        public string device_id;
    }
}
