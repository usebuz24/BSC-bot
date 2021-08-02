using System;
using System.Collections.Generic;

#nullable disable

namespace TelegramCryptoChatBot.Models
{
    public partial class User
    {
        public User(string UserId, string Username, string CurrentState)
        {
            this.UserId = UserId;
            this.Username = Username;
            this.CurrentState = CurrentState;
        }
        public User(string UserId)
        {
            this.UserId = UserId;
            this.Username = "";
            this.CurrentState = "0";
        }
        public User()
        {
            Choosencontracts = new HashSet<Choosencontract>();
        }

        public string UserId { get; set; }
        public string Username { get; set; }
        public string CurrentState { get; set; }

        public virtual ICollection<Choosencontract> Choosencontracts { get; set; }
    }
}
