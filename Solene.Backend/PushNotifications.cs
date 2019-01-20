using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Solene.Backend
{
    public static class PushNotifications
    {
        public static async Task SendPushNotification(Guid userId,
            string title, string body,
            ILogger logger)
        {
            string connectionString = Environment.GetEnvironmentVariable("SOLENE_NOTIFICATION_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString(connectionString, "solene-mobile-app");
            NotificationOutcome result = await hub.SendTemplateNotificationAsync(new Dictionary<string, string>
                {{"title", title },{"body", body }}, 
                userId.ToString("N"));
            logger.LogInformation($"Notification sent to {userId}. Outcome of push ID {result.TrackingId}: {result.State}.");
        }

        public static async Task<bool> Register(Guid userId, 
            PushNotificationPlatform platform, 
            string pnsToken, 
            string template, 
            ILogger logger)
        {
            string connectionString = Environment.GetEnvironmentVariable("SOLENE_NOTIFICATION_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString(connectionString, "solene-mobile-app");

            if (await hub.InstallationExistsAsync(ToInstallId(userId, platform)))
            {
                return true;
            }

            try
            {
                await hub.CreateOrUpdateInstallationAsync(new Installation
                {
                    InstallationId = ToInstallId(userId, platform),
                    Platform = (NotificationPlatform)platform,
                    PushChannel = pnsToken,
                    Tags = new[] { userId.ToString("N") },
                    Templates = new Dictionary<string, InstallationTemplate>
                    {
                        {"questionTemplate", new InstallationTemplate {
                            Body = template,
                            Headers = platform == PushNotificationPlatform.Windows
                                ? new Dictionary<string, string>{{ "X-WNS-Type", "wns/toast" }}
                                : null,
                        }}
                    }
                });

                return true;
            }
            catch(Exception ex)
            {
                logger.LogError($"Failed to register push notifications for {userId}. Error: {ex.Message}");
                return false;
            }
        }

        private static string ToInstallId(Guid userId, PushNotificationPlatform platform)
        {
            return $"{userId.ToString("N")}-{platform}";
        }
    }
}
