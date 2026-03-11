using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IOrderRepository
    {
        Task<PurchaseCartResponse?> PurchaseCart(Guid userId);
        Task<CheckoutSummaryResponse?> GetCheckoutSummary(Guid userId);
    }
}
