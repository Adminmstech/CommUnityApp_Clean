using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Models
{
    public class Orders
    {
    }
    public class PurchaseCartResponse
    {
        public int OrderId { get; set; }

        public decimal CartTotal { get; set; }

        public int CoinsUsed { get; set; }

        public int CoinsEarned { get; set; }

        public decimal FinalAmount { get; set; }
    }

    public class CheckoutSummaryResponse
    {
        public decimal CartTotal { get; set; }

        public int CoinsAvailable { get; set; }

        public int CoinsUsed { get; set; }

        public decimal CoinDiscount { get; set; }

        public decimal FinalAmount { get; set; }

        public int CoinsEarned { get; set; }
    }
}
