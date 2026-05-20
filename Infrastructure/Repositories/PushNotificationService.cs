using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class PushNotificationService : IPushNotificationService
    {
        public async Task SendAsync(string deviceToken, string title, string message)
        {
            var pushMessage = new Message()
            {
                Token = deviceToken,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = message
                }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(pushMessage);
        }
    }
}
