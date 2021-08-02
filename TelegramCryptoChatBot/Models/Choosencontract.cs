using System;
using System.Collections.Generic;

#nullable disable

namespace TelegramCryptoChatBot.Models
{
    public partial class Choosencontract
    {
        public Choosencontract(string Ticker, string Contract, string UserId)
        {
            this.Ticker = Ticker;
            this.Contract = Contract;
            this.UserId = UserId;
        }
        public string Ticker { get; set; }
        public string Contract { get; set; }
        public string UserId { get; set; }
        public int Id { get; set; }

        public virtual User User { get; set; }
    }
}
