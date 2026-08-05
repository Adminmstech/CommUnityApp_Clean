using CommUnityApp.ApplicationCore.BAL;
using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;

namespace CommUnityApp.UnitTests
{
    public class PushNotificationsBalTests
    {
        [Fact]
        public void EveryTriggerHasATemplate()
        {
            var templates = PushNotificationTemplates.All;

            foreach (var trigger in Enum.GetValues<PushNotificationTrigger>())
            {
                Assert.Contains(templates, x => x.Trigger == trigger);
            }
        }

        [Fact]
        public void RenderBody_ReplacesAllSupportedPlaceholders()
        {
            var bal = CreateBal();
            var data = new Dictionary<string, string>
            {
                ["Community Name"] = "Indo Communities",
                ["CommunityName"] = "Indo Communities",
                ["Event Name"] = "Diwali Night",
                ["EventName"] = "Diwali Night",
                ["Member Name"] = "Anika",
                ["MemberName"] = "Anika",
                ["Business Name"] = "Harbour Spice",
                ["BusinessName"] = "Harbour Spice",
                ["Promotion Name"] = "Weekend Saver",
                ["PromotionName"] = "Weekend Saver",
                ["Campaign Name"] = "Festive Drive",
                ["CampaignName"] = "Festive Drive",
                ["Member ID"] = "MEM-100",
                ["MemberID"] = "MEM-100",
                ["Coins"] = "50"
            };

            foreach (var trigger in Enum.GetValues<PushNotificationTrigger>())
            {
                var body = bal.RenderBody(trigger, data);

                Assert.DoesNotContain("{", body);
                Assert.DoesNotContain("}", body);
            }
        }

        [Fact]
        public async Task TriggerAsync_SendsExplicitDeviceTokens()
        {
            var dal = new FakePushNotificationsDal();
            var sender = new FakePushNotificationService();
            var bal = new PushNotificationsBal(dal, sender);

            var result = await bal.TriggerAsync(new PushNotificationTriggerRequest
            {
                Trigger = PushNotificationTrigger.BusinessAddedToFavourites,
                DeviceTokens = new List<string> { "token-1", "token-1", "token-2" },
                Data = new Dictionary<string, string>
                {
                    ["Business Name"] = "Harbour Spice"
                }
            });

            Assert.Equal(2, result.RecipientCount);
            Assert.Equal(2, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
            Assert.Equal(2, sender.Sent.Count);
            Assert.Contains("Harbour Spice", result.Body);
        }

        private static PushNotificationsBal CreateBal()
        {
            return new PushNotificationsBal(new FakePushNotificationsDal(), new FakePushNotificationService());
        }

        private sealed class FakePushNotificationsDal : IPushNotificationsDal
        {
            public Task<IReadOnlyList<PushNotificationRecipient>> GetRecipientsAsync(PushNotificationRecipientQuery query)
            {
                IReadOnlyList<PushNotificationRecipient> recipients = new List<PushNotificationRecipient>
                {
                    new() { UserId = Guid.NewGuid(), DeviceToken = "token-1" }
                };

                return Task.FromResult(recipients);
            }

            public Task SaveDispatchLogAsync(PushNotificationTriggerRequest request, PushNotificationDispatchResult result)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class FakePushNotificationService : IPushNotificationService
        {
            public List<(string DeviceToken, string Title, string Message)> Sent { get; } = new();

            public Task SendAsync(string deviceToken, string title, string message)
            {
                Sent.Add((deviceToken, title, message));
                return Task.CompletedTask;
            }
        }
    }
}
