using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;

namespace Solene.MobileApp.Core.Services
{
    public class WhatsNewService
    {
        private static Dictionary<Version, string[]> _changelog = new Dictionary<Version, string[]>
        {
            { new Version(1, 0), new[] { "Initial release." } },
            { new Version(1, 1), new[]
                {
                    "Introduced a settings page.",
                    "Moved most of the commands in the toolbar menu on the profile overview page into the settings page.",
                    "Added the ability to toggle public visibility of your adventure."
                }
            }
        };

        public WhatsNewService()
        {
            VersionTracking.Track();
        }

        /// <summary>
        /// Gets a list of all changes since the last-seen version. Can return null.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Version, string[]> GetChangelogSinceLastLaunch()
        {
            if (!VersionTracking.IsFirstLaunchForCurrentVersion)
            {
                return null;
            }

            string lastSeenVersionString = VersionTracking.PreviousVersion;
            if (lastSeenVersionString == null)
            {
                lastSeenVersionString = "1.0.0";
            }
            int[] splitLastVersion = lastSeenVersionString.Split('.').Select(int.Parse).ToArray();
            Version lastSeenVersion = new Version(splitLastVersion[0], splitLastVersion[1], splitLastVersion[2]);
            Dictionary<Version, string[]> changesSinceLastLaunch = _changelog
                .Where(x => x.Key.Major > lastSeenVersion.Major || x.Key.Minor > lastSeenVersion.Minor)
                .ToDictionary(x => x.Key, y => y.Value);
            if (!changesSinceLastLaunch.Any())
            {
                return null;
            }

            return changesSinceLastLaunch;
        }

    }
}
