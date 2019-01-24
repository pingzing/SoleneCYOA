using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using Solene.MobileApp.Core.Extensions;
using Solene.MobileApp.Core.Messages;
using Solene.MobileApp.Core.Models;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Solene.MobileApp.Core.Services
{
    public interface IProfileService
    {
        IEnumerable<ProfileMoniker> GetSavedProfileNames();
        Task<MaybeResult<PlayerProfile, GenericErrorResult>> GetProfile(Guid id);
        Task SaveProfile(PlayerProfile profile);
        Task AddQuestionToSavedProfile(Guid profileId, Question newQuestion);
    }

    public class ProfileService : IProfileService
    {
        private readonly IMessenger _messagingService;
        private static readonly string _appDataDir = FileSystem.AppDataDirectory;

        public ProfileService(IMessenger messagingService)
        {
            _messagingService = messagingService;
        }

        public IEnumerable<ProfileMoniker> GetSavedProfileNames()
        {
            return Directory.EnumerateFiles(_appDataDir, "*.json", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Select(x =>
                {
                    var idAndName = x.Split('_');
                    return new ProfileMoniker { Id = Guid.Parse(idAndName[0]), CharacterName = idAndName[1] };
                });
        }

        public async Task<MaybeResult<PlayerProfile, GenericErrorResult>> GetProfile(Guid id)
        {
            var profilePath = Directory.EnumerateFiles(_appDataDir, "*.json", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .FirstOrDefault(x =>
                {
                    var idAndName = x.Split('_');
                    return id == Guid.Parse(idAndName[0]);
                });

            if (profilePath == null)
            {
                return MaybeResult<PlayerProfile, GenericErrorResult>.CreateError(GenericErrorResult.NotFound);
            }

            var profile = File.OpenRead(Path.Combine(_appDataDir, profilePath))
                .DeserializeJsonFromStream<PlayerProfile>();

            return MaybeResult<PlayerProfile, GenericErrorResult>.CreateOk(profile);            
        }

        public async Task SaveProfile(PlayerProfile profile)
        {
            string profileJson = JsonConvert.SerializeObject(profile);
            using (var writer = File.CreateText(Path.Combine(_appDataDir,
                $"{profile.PlayerInfo.Id.ToString("N")}" +
                "_" +
                $"{SanitizeFileName(profile.PlayerInfo.Name)}.json")))
            {
                await writer.WriteAsync(profileJson);
            }
        }

        public async Task AddQuestionToSavedProfile(Guid profileId, Question newQuestion)
        {
            MaybeResult<PlayerProfile, GenericErrorResult> getProfileResult = await GetProfile(profileId);
            if (getProfileResult.IsError)
            {
                Debug.WriteLine($"Couldn't fine a profile with ID {profileId}");
                return;
            }

            PlayerProfile profile = getProfileResult.Unwrap();
            profile.Questions.Add(newQuestion);
            await SaveProfile(profile);
            _messagingService.Send(new ProfileUpdated { NewQuestion = newQuestion, ProfileId = profileId });
        }

        private readonly static Regex _illegalCharsRegex = new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", RegexOptions.Compiled);        
        public static string SanitizeFileName(string inputString)
        {
            if (_illegalCharsRegex.IsMatch(inputString))
            {
                string oldInput = inputString;
                inputString = _illegalCharsRegex.Replace(inputString, "");
                Debug.WriteLine($"Sanitizing {oldInput} into {inputString}");
            }

            return inputString;
        }
    }
}
