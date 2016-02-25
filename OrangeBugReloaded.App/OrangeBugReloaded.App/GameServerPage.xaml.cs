using OrangeBugReloaded.App.Common;
using OrangeBugReloaded.App.ViewModels;
using OrangeBugReloaded.Core;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
            KeyboardManager.ArrowKeyDown.Subscribe(OnDirectionKeyDown);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var port = e.Parameter.ToString();
            ServerViewModel = new GameServerViewModel(canvas);
            await ServerViewModel.OpenForNetworkAsync(port);

            ClientViewModel = new LocalGameClientViewModel(clientCanvas, ServerViewModel.LocalServer);
        }

        private async void OnDirectionKeyDown(Point direction)
        {
            await ClientViewModel.Client.MovePlayerAsync(direction);
        }

        private void canvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(sender as UIElement).Properties.MouseWheelDelta / 200f;
            var normalizedDelta = Mathf.Abs(delta) + 1;

            if (delta >= 0)
                ServerViewModel.Renderer.ZoomLevel = Mathf.Clamp(ServerViewModel.Renderer.ZoomLevel * normalizedDelta, 10, 200);
            else
                ServerViewModel.Renderer.ZoomLevel = Mathf.Clamp(ServerViewModel.Renderer.ZoomLevel / normalizedDelta, 10, 200);
        }
    }
}
