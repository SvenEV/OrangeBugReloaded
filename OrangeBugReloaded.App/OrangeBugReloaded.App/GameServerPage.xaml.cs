using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.App.Presentation;
using OrangeBugReloaded.App.ViewModels;
using OrangeBugReloaded.Core;
using System;
using System.Numerics;
using System.Threading.Tasks;
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

        private async void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var canvas = (CanvasAnimatedControl)sender;
            var currentPoint = e.GetCurrentPoint(canvas);
            await PerformBasicTouchNavigation(canvas, currentPoint.Position.ToVector2());
        }

        private async Task PerformBasicTouchNavigation(CanvasAnimatedControl canvas, Vector2 position)
        {
            var x = position.X / canvas.Size.Width * 2 - 1;
            var y = position.Y / canvas.Size.Height * 2 - 1;
            Point direction;

            if (Math.Abs(x) < Math.Abs(y))
            {
                direction = y < 0 ? Point.North : Point.South;
            }
            else
            {
                direction = x < 0 ? Point.West : Point.East;
            }

            await ClientViewModel.Client.MovePlayerAsync(direction);
            ClientViewModel.Renderer.CameraPosition = ClientViewModel.Client.PlayerPosition.ToVector2();
        }
    }
}
