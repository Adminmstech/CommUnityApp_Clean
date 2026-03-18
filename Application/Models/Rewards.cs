using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    public class Rewards
    {
        public Guid UserId { get; set; }

        public int TotalCoinsEarned { get; set; }

        public int TotalCoinsUsed { get; set; }

        public int? BalanceCoins { get; set; }

        public decimal? MoneyValue { get; set; }
    }
}
