using CommUnityApp.ApplicationCore.Models;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IPushNotificationsDal
    {
        Task<IReadOnlyList<PushNotificationRecipient>> GetRecipientsAsync(PushNotificationRecipientQuery query);
        Task SaveDispatchLogAsync(PushNotificationTriggerRequest request, PushNotificationDispatchResult result);
    }
}
