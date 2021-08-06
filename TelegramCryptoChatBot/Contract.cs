using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramCryptoChatBot
{
    class Contract
    {
        public long updated_at { get; set; }
        public Data data { get; set; }
    }
    class Data
    {
        public string name { get; set; }
        public string symbol { get; set; }
        public string price { get; set; }
        public string price_BNB { get; set; }
    }
}
