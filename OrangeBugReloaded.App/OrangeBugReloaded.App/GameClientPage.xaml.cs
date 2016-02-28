using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.App.Presentation;
using OrangeBugReloaded.App.ViewModels;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.ClientServer.Net;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
            KeyboardManager.ArrowKeyDown.Subscribe(OnDirectionKeyDown);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var serverInfo = e.Parameter as NetGameServerInfo;

            if (serverInfo == null)
                throw new ArgumentException($"Expected {nameof(NetGameServerInfo)} as parameter");

            ViewModel = new NetGameClientViewModel(canvas, serverInfo);
        }

        private async void OnDirectionKeyDown(Point direction)
        {
            await ViewModel.Client.MovePlayerAsync(direction);
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
