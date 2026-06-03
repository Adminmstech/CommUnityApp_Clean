using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IPushNotificationService
    {
        Task SendAsync(string deviceToken, string title, string message);
    }
}
