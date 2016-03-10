using OrangeBugReloaded.App.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace OrangeBugReloaded.App
{
    public sealed partial class GameServerPage : Page
    {
        public GameServerViewModel ServerViewModel { get; private set; }

        public LocalGameClientViewModel ClientViewModel { get; private set; }

        public GameServerPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Start server
            var port = e.Parameter.ToString();
            ServerViewModel = new GameServerViewModel();
            await ServerViewModel.InitializeAsync();
            await ServerViewModel.OpenForNetworkAsync(port);
            serverPresenter.Map = ServerViewModel.LocalServer.UnderlyingGameServer.Map;

            // Connect local client
            ClientViewModel = new LocalGameClientViewModel(ServerViewModel.LocalServer);
            await ClientViewModel.InitializeAsync();
            clientPresenter.Map = ClientViewModel.Client.Map;
            clientPresenter.GameClient = ClientViewModel.Client;
        }
    }
}
