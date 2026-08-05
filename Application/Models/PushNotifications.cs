using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CommUnityApp.ApplicationCore.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PushNotificationTrigger
    {
        UserJoinsCommunity,
        CommunityUpdatePublished,
        NewEventPublished,
        EventUpdated,
        EventCancelled,
        EventReminderOneDay,
        EventReminderOneHour,
        VolunteerOpportunityPosted,
        HelpingHandsRequestPosted,
        IndoCoinsEarned,
        NewMemberJoined,
        NewCommunityUpdatePublished,
        NewEventRegistration,
        EventNearingCapacity,
        EventCompleted,
        BusinessAddedToFavourites,
        BusinessRemovedFromFavourites,
        NewPromotionPublished,
        NewCampaignLaunched,
        PromotionEndingSoon,
        CampaignEndingSoon,
        PromoCodeRedeemedSuccessfully,
        CampaignCodeRedeemedSuccessfully,
        IndoCoinsCredited,
        PromotionPublished,
        CampaignPublished,
        MemberParticipatedPromo,
        MemberParticipatedCampaign,
        MemberRedeemedPromotion,
        MemberRedeemedCampaign,
        IndoCoinsIssued,
        BusinessAdded,
        NewPromotionCreated,
        NewCampaignCreated,
        HighPromotionRedemption,
        CampaignCompleted,
        BusinessDeactivated
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PushNotificationRecipientScope
    {
        ExplicitUsers,
        CommunityMembers,
        EventRegisteredMembers,
        BusinessAdmin,
        SuperAdmins,
        AllMembers
    }

    public class PushNotificationTemplate
    {
        public PushNotificationTrigger Trigger { get; set; }
        public PushNotificationRecipientScope DefaultRecipientScope { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public class PushNotificationRecipient
    {
        public Guid? UserId { get; set; }
        public string? DeviceToken { get; set; }
        public string? RecipientType { get; set; }
    }

    public class PushNotificationRecipientQuery
    {
        public PushNotificationRecipientScope Scope { get; set; }
        public IReadOnlyCollection<Guid> UserIds { get; set; } = Array.Empty<Guid>();
        public int? CommunityId { get; set; }
        public int? EventId { get; set; }
        public int? BusinessId { get; set; }
    }

    public class PushNotificationTriggerRequest
    {
        public PushNotificationTrigger Trigger { get; set; }
        public PushNotificationRecipientScope? RecipientScope { get; set; }
        public List<Guid> UserIds { get; set; } = new();
        public List<string> DeviceTokens { get; set; } = new();
        public int? CommunityId { get; set; }
        public int? EventId { get; set; }
        public int? BusinessId { get; set; }
        public Dictionary<string, string> Data { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string? TitleOverride { get; set; }
        public string? BodyOverride { get; set; }
    }

    public class PushNotificationPreviewRequest
    {
        public PushNotificationTrigger Trigger { get; set; }
        public Dictionary<string, string> Data { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string? BodyOverride { get; set; }
    }

    public class PushNotificationDispatchResult
    {
        public int ResultId { get; set; }
        public string ResultMessage { get; set; } = string.Empty;
        public PushNotificationTrigger Trigger { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int RecipientCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
