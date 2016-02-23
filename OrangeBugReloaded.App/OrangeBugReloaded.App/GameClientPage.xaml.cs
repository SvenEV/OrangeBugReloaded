using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.App.Common;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.ClientServer.Net.Client;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace OrangeBugReloaded.App
{
    public sealed partial class GameClientPage : Page
    {
        public GameClientViewModel ViewModel { get; private set; }

        public GameClientPage()
        {
            InitializeComponent();
            DataContext = this;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var serverInfo = e.Parameter as ServerConnectInfo;

            if (serverInfo == null)
                throw new ArgumentException($"Expected {nameof(ServerConnectInfo)} as parameter");

            ViewModel = new GameClientViewModel(canvas, serverInfo);
        }

        private async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.Left:
                    await ViewModel.Client.MovePlayerAsync(Point.West);
                    break;

                case VirtualKey.Right:
                    await ViewModel.Client.MovePlayerAsync(Point.East);
                    break;

                case VirtualKey.Up:
                    await ViewModel.Client.MovePlayerAsync(Point.North);
                    break;

                case VirtualKey.Down:
                    await ViewModel.Client.MovePlayerAsync(Point.South);
                    break;
            }

            ViewModel.Renderer.CameraPosition = ViewModel.Client.PlayerPosition.ToVector2();

            args.Handled = true;
        }

        private void canvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(canvas).Properties.MouseWheelDelta / 200f;
            var normalizedDelta = Mathf.Abs(delta) + 1;

            if (delta >= 0)
                ViewModel.Renderer.ZoomLevel = Mathf.Clamp(ViewModel.Renderer.ZoomLevel * normalizedDelta, 10, 200);
            else
                ViewModel.Renderer.ZoomLevel = Mathf.Clamp(ViewModel.Renderer.ZoomLevel / normalizedDelta, 10, 200);
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

            await ViewModel.Client.MovePlayerAsync(direction);
            ViewModel.Renderer.CameraPosition = ViewModel.Client.PlayerPosition.ToVector2();
        }
    }
}
