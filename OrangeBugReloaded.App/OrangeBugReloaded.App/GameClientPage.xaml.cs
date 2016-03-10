using OrangeBugReloaded.App.ViewModels;
using OrangeBugReloaded.Core.ClientServer.Net;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace OrangeBugReloaded.App
{
    public sealed partial class GameClientPage : Page
    {
        public NetGameClientViewModel ViewModel { get; private set; }

        public GameClientPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var serverInfo = e.Parameter as NetGameServerInfo;

            if (serverInfo == null)
                throw new ArgumentException($"Expected {nameof(NetGameServerInfo)} as parameter");

            ViewModel = new NetGameClientViewModel(serverInfo);
            await ViewModel.InitializeAsync();

            presenter.Map = ViewModel.Client.Map;
            presenter.GameClient = ViewModel.Client;
        }
    }
}
