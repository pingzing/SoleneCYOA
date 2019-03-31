using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Solene.Backend
{
    public static class PushNotifications
    {
        public static async Task SendPushNotification(uint sequenceNumber, 
            Guid userId,
            string title, string body, string questionJson,
            ILogger logger)
        {
            // TODO: The proper way to do this would be to get the template that this notification will be using,
            // and actually calculate how much space we have left over for the compressed question,
            // but for now, just use a fairly conservative lowball value of 2500.
            string compressedBase64Question = CompressAndBase64Question(questionJson, 2500);

            string connectionString = Environment.GetEnvironmentVariable("SOLENE_NOTIFICATION_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString(connectionString, "solene-mobile-app", true);            
            NotificationOutcome result = await hub.SendTemplateNotificationAsync(new Dictionary<string, string>
                { {"title", $"{sequenceNumber}: {title}" }, {"body", body }, {"question", compressedBase64Question} }, 
                userId.ToString("N"));            

            logger.LogInformation($"Notification sent to {userId}. Outcome of push ID {result.TrackingId}: {result.State}.");
        }

        private static string CompressAndBase64Question(string questionJson, uint maxSize)
        {
            byte[] compressedBytes;
            var questionBytes = Encoding.UTF8.GetBytes(questionJson);
            using (var inputStream = new MemoryStream(questionBytes))
            using (var outputStream = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    inputStream.CopyTo(compressionStream);
                }
                compressedBytes = outputStream.ToArray();
            }

            string base64String = Convert.ToBase64String(compressedBytes);
            if (base64String.Length > maxSize)
            {
                return null;
            }

            return base64String;
        }

        public static async Task<bool> Register(Guid userId, 
            PushNotificationPlatform platform, 
            string pnsToken, 
            string template, 
            ILogger logger)
        {
            string connectionString = Environment.GetEnvironmentVariable("SOLENE_NOTIFICATION_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString(connectionString, "solene-mobile-app");            

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

                logger.LogInformation($"Registered user {ToInstallId(userId, platform)} for push notifications.");
                return true;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"Failed to register push notifications for {userId}.", null);
                return false;
            }
        }

        private static string ToInstallId(Guid userId, PushNotificationPlatform platform)
        {
            return $"{userId.ToString("N")}-{platform}";
        }
    }
}
