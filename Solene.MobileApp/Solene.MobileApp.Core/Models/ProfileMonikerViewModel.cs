using System;

namespace Solene.MobileApp.Core.Models
{
    public class ProfileMonikerViewModel
    {
        public string CharacterName { get; set; }
        public Guid Id { get; set; }

        public string CharacterNameString => CharacterName.Replace(".json", "");
        public string IdString => $"ID: {Id.ToString("N")}";

        public ProfileMonikerViewModel(ProfileMoniker moniker)
        {
            CharacterName = moniker.CharacterName;
            Id = moniker.Id;
        }
    }
}
