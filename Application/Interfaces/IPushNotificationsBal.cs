using CommUnityApp.ApplicationCore.Models;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IPushNotificationsBal
    {
        IReadOnlyCollection<PushNotificationTemplate> GetTemplates();
        PushNotificationTemplate GetTemplate(PushNotificationTrigger trigger);
        string RenderBody(PushNotificationTrigger trigger, IReadOnlyDictionary<string, string> data, string? bodyOverride = null);
        Task<PushNotificationDispatchResult> TriggerAsync(PushNotificationTriggerRequest request);
    }
}
