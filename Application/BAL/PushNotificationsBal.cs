using System.Text.RegularExpressions;
using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;

namespace CommUnityApp.ApplicationCore.BAL
{
    public class PushNotificationsBal : IPushNotificationsBal
    {
        private static readonly Regex PlaceholderRegex = new(@"\{([^{}]+)\}", RegexOptions.Compiled);
        private readonly IPushNotificationsDal _pushNotificationsDal;
        private readonly IPushNotificationService _pushNotificationService;

        public PushNotificationsBal(
            IPushNotificationsDal pushNotificationsDal,
            IPushNotificationService pushNotificationService)
        {
            _pushNotificationsDal = pushNotificationsDal;
            _pushNotificationService = pushNotificationService;
        }

        public IReadOnlyCollection<PushNotificationTemplate> GetTemplates()
        {
            return PushNotificationTemplates.All;
        }

        public PushNotificationTemplate GetTemplate(PushNotificationTrigger trigger)
        {
            return PushNotificationTemplates.Get(trigger);
        }

        public string RenderBody(
            PushNotificationTrigger trigger,
            IReadOnlyDictionary<string, string> data,
            string? bodyOverride = null)
        {
            var template = bodyOverride ?? GetTemplate(trigger).Body;
            return Render(template, data);
        }

        public async Task<PushNotificationDispatchResult> TriggerAsync(PushNotificationTriggerRequest request)
        {
            var template = GetTemplate(request.Trigger);
            var title = string.IsNullOrWhiteSpace(request.TitleOverride)
                ? template.Title
                : request.TitleOverride.Trim();
            var body = Render(request.BodyOverride ?? template.Body, request.Data);

            var recipients = await ResolveRecipientsAsync(request, template);
            var result = new PushNotificationDispatchResult
            {
                ResultId = 1,
                ResultMessage = "Push notification dispatch completed.",
                Trigger = request.Trigger,
                Title = title,
                Body = body,
                RecipientCount = recipients.Count
            };

            foreach (var recipient in recipients)
            {
                try
                {
                    await _pushNotificationService.SendAsync(recipient.DeviceToken!, title, body);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add(ex.Message);
                }
            }

            if (result.RecipientCount == 0)
            {
                result.ResultId = 0;
                result.ResultMessage = "No push notification recipients found.";
            }
            else if (result.FailureCount > 0 && result.SuccessCount == 0)
            {
                result.ResultId = 0;
                result.ResultMessage = "Push notification dispatch failed for all recipients.";
            }

            await _pushNotificationsDal.SaveDispatchLogAsync(request, result);
            return result;
        }

        private async Task<List<PushNotificationRecipient>> ResolveRecipientsAsync(
            PushNotificationTriggerRequest request,
            PushNotificationTemplate template)
        {
            if (request.DeviceTokens.Count > 0)
            {
                return request.DeviceTokens
                    .Where(token => !string.IsNullOrWhiteSpace(token))
                    .Select(token => new PushNotificationRecipient
                    {
                        DeviceToken = token.Trim(),
                        RecipientType = "ExplicitDeviceToken"
                    })
                    .DistinctBy(x => x.DeviceToken)
                    .ToList();
            }

            var query = new PushNotificationRecipientQuery
            {
                Scope = request.RecipientScope ?? template.DefaultRecipientScope,
                UserIds = request.UserIds.Where(x => x != Guid.Empty).ToList(),
                CommunityId = request.CommunityId,
                EventId = request.EventId,
                BusinessId = request.BusinessId
            };

            var recipients = await _pushNotificationsDal.GetRecipientsAsync(query);

            return recipients
                .Where(x => !string.IsNullOrWhiteSpace(x.DeviceToken))
                .DistinctBy(x => x.DeviceToken)
                .ToList();
        }

        private static string Render(string template, IReadOnlyDictionary<string, string> data)
        {
            var values = new Dictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);

            foreach (var pair in data)
            {
                values.TryAdd(pair.Key.Replace(" ", string.Empty), pair.Value);
            }

            return PlaceholderRegex.Replace(template, match =>
            {
                var key = match.Groups[1].Value.Trim();
                return values.TryGetValue(key, out var value) ? value : match.Value;
            });
        }
    }

    public static class PushNotificationTemplates
    {
        private static readonly IReadOnlyDictionary<PushNotificationTrigger, PushNotificationTemplate> Templates =
            new List<PushNotificationTemplate>
            {
                Template(PushNotificationTrigger.UserJoinsCommunity, PushNotificationRecipientScope.ExplicitUsers, "Community", "🎉 Welcome to {Community Name}! Stay connected with updates and events."),
                Template(PushNotificationTrigger.CommunityUpdatePublished, PushNotificationRecipientScope.CommunityMembers, "Community Update", "📢 New update from {Community Name}. Tap to view."),
                Template(PushNotificationTrigger.NewEventPublished, PushNotificationRecipientScope.CommunityMembers, "New Event", "🎉 A new event is now open for registration."),
                Template(PushNotificationTrigger.EventUpdated, PushNotificationRecipientScope.EventRegisteredMembers, "Event Updated", "📅 {Event Name} has been updated. Check the latest details."),
                Template(PushNotificationTrigger.EventCancelled, PushNotificationRecipientScope.EventRegisteredMembers, "Event Cancelled", "❗ {Event Name} has been cancelled."),
                Template(PushNotificationTrigger.EventReminderOneDay, PushNotificationRecipientScope.EventRegisteredMembers, "Event Reminder", "⏰ Reminder: {Event Name} starts tomorrow."),
                Template(PushNotificationTrigger.EventReminderOneHour, PushNotificationRecipientScope.EventRegisteredMembers, "Event Reminder", "🚀 {Event Name} starts in 1 hour."),
                Template(PushNotificationTrigger.VolunteerOpportunityPosted, PushNotificationRecipientScope.CommunityMembers, "Volunteer Opportunity", "🤝 A new volunteer opportunity is available in your community."),
                Template(PushNotificationTrigger.HelpingHandsRequestPosted, PushNotificationRecipientScope.CommunityMembers, "Helping Hands", "❤️ A community member needs assistance. Can you help?"),
                Template(PushNotificationTrigger.IndoCoinsEarned, PushNotificationRecipientScope.ExplicitUsers, "IndoCoins Earned", "🪙 You've earned {Coins} IndoCoins for participating in {Event Name}."),
                Template(PushNotificationTrigger.NewMemberJoined, PushNotificationRecipientScope.CommunityMembers, "New Member", "👋 {Member Name} has joined your community."),
                Template(PushNotificationTrigger.NewCommunityUpdatePublished, PushNotificationRecipientScope.CommunityMembers, "Community Update", "📢 A new community update has been published successfully."),
                Template(PushNotificationTrigger.NewEventRegistration, PushNotificationRecipientScope.BusinessAdmin, "Event Registration", "🎟️ {Member Name} registered for {Event Name}."),
                Template(PushNotificationTrigger.EventNearingCapacity, PushNotificationRecipientScope.BusinessAdmin, "Event Capacity", "⚠️ {Event Name} is almost full."),
                Template(PushNotificationTrigger.EventCompleted, PushNotificationRecipientScope.BusinessAdmin, "Event Completed", "✅ {Event Name} has been completed successfully."),
                Template(PushNotificationTrigger.BusinessAddedToFavourites, PushNotificationRecipientScope.ExplicitUsers, "Favourite Added", "⭐ {Business Name} has been added to your Favourites."),
                Template(PushNotificationTrigger.BusinessRemovedFromFavourites, PushNotificationRecipientScope.ExplicitUsers, "Favourite Removed", "💔 {Business Name} has been removed from your Favourites."),
                Template(PushNotificationTrigger.NewPromotionPublished, PushNotificationRecipientScope.AllMembers, "New Promotion", "🛍️ Exclusive offer available! Explore the latest promotion from {Business Name}."),
                Template(PushNotificationTrigger.NewCampaignLaunched, PushNotificationRecipientScope.AllMembers, "New Campaign", "📢 {Business Name} has launched a new campaign. Check it out!"),
                Template(PushNotificationTrigger.PromotionEndingSoon, PushNotificationRecipientScope.AllMembers, "Promotion Ending Soon", "⏰ Hurry! {Promotion Name} ends in 2 days."),
                Template(PushNotificationTrigger.CampaignEndingSoon, PushNotificationRecipientScope.AllMembers, "Campaign Ending Soon", "⏰ Last chance to participate in {Campaign Name}."),
                Template(PushNotificationTrigger.PromoCodeRedeemedSuccessfully, PushNotificationRecipientScope.ExplicitUsers, "Promo Code Redeemed", "✅ Promo code redeemed successfully. Enjoy your offer!"),
                Template(PushNotificationTrigger.CampaignCodeRedeemedSuccessfully, PushNotificationRecipientScope.ExplicitUsers, "Campaign Code Redeemed", "✅ Campaign code redeemed successfully."),
                Template(PushNotificationTrigger.IndoCoinsCredited, PushNotificationRecipientScope.ExplicitUsers, "IndoCoins Credited", "🪙 {Coins} IndoCoins have been added to your wallet."),
                Template(PushNotificationTrigger.PromotionPublished, PushNotificationRecipientScope.BusinessAdmin, "Promotion Published", "🎉 Your promotion is now live."),
                Template(PushNotificationTrigger.CampaignPublished, PushNotificationRecipientScope.BusinessAdmin, "Campaign Published", "📢 Your campaign has been published successfully."),
                Template(PushNotificationTrigger.MemberParticipatedPromo, PushNotificationRecipientScope.BusinessAdmin, "Promotion Interest", "{Member ID} showed interest in Promotion"),
                Template(PushNotificationTrigger.MemberParticipatedCampaign, PushNotificationRecipientScope.BusinessAdmin, "Campaign Interest", "{Member ID} showed interest in Campaign"),
                Template(PushNotificationTrigger.MemberRedeemedPromotion, PushNotificationRecipientScope.BusinessAdmin, "Promotion Redeemed", "🎟️ {Member ID} redeemed your promotion code."),
                Template(PushNotificationTrigger.MemberRedeemedCampaign, PushNotificationRecipientScope.BusinessAdmin, "Campaign Redeemed", "🎟️ {Member ID} redeemed your campaign code."),
                Template(PushNotificationTrigger.IndoCoinsIssued, PushNotificationRecipientScope.BusinessAdmin, "IndoCoins Issued", "🪙 {Coins} IndoCoins have been rewarded to the member."),
                Template(PushNotificationTrigger.BusinessAdded, PushNotificationRecipientScope.SuperAdmins, "Business Added", "✅ {Business Name} added successfully."),
                Template(PushNotificationTrigger.NewPromotionCreated, PushNotificationRecipientScope.SuperAdmins, "New Promotion Created", "🎉 A new promotion has been created by {Business Name}."),
                Template(PushNotificationTrigger.NewCampaignCreated, PushNotificationRecipientScope.SuperAdmins, "New Campaign Created", "📢 A new campaign has been created by {Business Name}."),
                Template(PushNotificationTrigger.HighPromotionRedemption, PushNotificationRecipientScope.SuperAdmins, "High Promotion Redemption", "🔥 {Promotion Name} is receiving high member engagement."),
                Template(PushNotificationTrigger.CampaignCompleted, PushNotificationRecipientScope.SuperAdmins, "Campaign Completed", "✅ {Campaign Name} has ended successfully."),
                Template(PushNotificationTrigger.BusinessDeactivated, PushNotificationRecipientScope.SuperAdmins, "Business Deactivated", "⚠️ {Business Name} has been deactivated.")
            }.ToDictionary(x => x.Trigger);

        public static IReadOnlyCollection<PushNotificationTemplate> All => Templates.Values.ToList();

        public static PushNotificationTemplate Get(PushNotificationTrigger trigger)
        {
            if (Templates.TryGetValue(trigger, out var template))
                return template;

            throw new ArgumentOutOfRangeException(nameof(trigger), trigger, "Push notification trigger is not configured.");
        }

        private static PushNotificationTemplate Template(
            PushNotificationTrigger trigger,
            PushNotificationRecipientScope scope,
            string title,
            string body)
        {
            return new PushNotificationTemplate
            {
                Trigger = trigger,
                DefaultRecipientScope = scope,
                Title = title,
                Body = body
            };
        }
    }
}
