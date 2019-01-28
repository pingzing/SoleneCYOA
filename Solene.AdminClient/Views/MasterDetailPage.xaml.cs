using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Solene.AdminClient.Consts;
using Solene.AdminClient.Models;
using Solene.AdminClient.Services;
using Solene.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Solene.AdminClient.Views
{
    public sealed partial class MasterDetailPage : Page, INotifyPropertyChanged
    {
        private AdminPlayerProfile _selected;
        public AdminPlayerProfile Selected
        {
            get { return _selected; }
            set { Set(ref _selected, value); }
        }

        public ObservableCollection<AdminPlayerProfile> Players { get; private set; } = new ObservableCollection<AdminPlayerProfile>();

        public MasterDetailPage()
        {
            InitializeComponent();
            Loaded += MasterDetailPage_Loaded;
        }

        private async void MasterDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            await Secrets.InitializeAsync();
            await Refresh();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        private async Task Refresh()
        {
            Players.Clear();
            var playersAndDetails = await NetworkService.GetAllPlayersAndQuestions();
            List<AdminPlayerProfile> profiles = playersAndDetails.AllPlayers.Select(p =>
                new AdminPlayerProfile
                {
                    PlayerInfo = p,
                    Questions = playersAndDetails.AllQuestions
                        .Where(q => q.PlayerId == p.Id)
                        .OrderBy(q => q.SequenceNumber)
                        .ToList()
                }
            ).OrderByDescending(x => x.Questions.Last().UpdatedTimestamp)
            .ToList();

           foreach (var item in profiles)
            {
                Players.Add(item);
            }

            if (MasterDetailsViewControl.ViewState == MasterDetailsViewState.Both)
            {
                Selected = Players.FirstOrDefault();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));             

        private async void MasterItem_DeleteClick(object sender, object e)
        {
            var selectedPlayer = e as AdminPlayerProfile;
            bool result = await NetworkService.DeletePlayer(selectedPlayer.PlayerInfo.Id);
            if (!result)
            {
                return;
            }

            Players.Remove(selectedPlayer);
        }
    }
}
