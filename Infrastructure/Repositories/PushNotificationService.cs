using CommUnityApp.ApplicationCore.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class PushNotificationService : IPushNotificationService
    {
        private static readonly HttpClient LegacyFirebaseClient = new(
            new HttpClientHandler
            {
                UseProxy = false
            });
        private readonly IConfiguration _configuration;

        public PushNotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string deviceToken, string title, string message)
        {
            if (FirebaseApp.DefaultInstance != null)
            {
                var pushMessage = new Message
                {
                    Token = deviceToken,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = message
                    }
                };

                await FirebaseMessaging.DefaultInstance.SendAsync(pushMessage);
                return;
            }

            var serverKey = _configuration["Firebase:ServerKey"];

            if (string.IsNullOrWhiteSpace(serverKey))
            {
                throw new InvalidOperationException(
                    "Firebase is not initialized. Configure Firebase:ServiceAccountPath, GOOGLE_APPLICATION_CREDENTIALS, or Firebase:ServerKey.");
            }

            await SendWithLegacyServerKeyAsync(deviceToken, title, message, serverKey);
        }

        private async Task SendWithLegacyServerKeyAsync(
            string deviceToken,
            string title,
            string message,
            string serverKey)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://fcm.googleapis.com/fcm/send");

            request.Headers.TryAddWithoutValidation("Authorization", $"key={serverKey}");
            request.Content = JsonContent.Create(new
            {
                to = deviceToken,
                notification = new
                {
                    title,
                    body = message
                },
                data = new
                {
                    title,
                    body = message
                }
            });

            using var response = await LegacyFirebaseClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Firebase legacy send failed with HTTP {(int)response.StatusCode}: {TrimFirebaseResponse(responseBody)}");
            }

            var legacyResponse = JsonSerializer.Deserialize<FirebaseLegacyResponse>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (legacyResponse != null && legacyResponse.Failure > 0)
            {
                var error = legacyResponse.Results?
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Error))
                    ?.Error;

                throw new InvalidOperationException(
                    $"Firebase legacy send failed: {error ?? TrimFirebaseResponse(responseBody)}");
            }
        }

        private static string TrimFirebaseResponse(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
                return "No response body.";

            return responseBody.Length <= 500
                ? responseBody
                : responseBody[..500];
        }

        private sealed class FirebaseLegacyResponse
        {
            public int Failure { get; set; }

            public List<FirebaseLegacyResult> Results { get; set; } = new();
        }

        private sealed class FirebaseLegacyResult
        {
            public string? Error { get; set; }
        }
    }
}
