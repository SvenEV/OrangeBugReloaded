using OrangeBugReloaded.Core;
using OrangeBugReloaded.App.Common;
using System;
using Windows.UI.Xaml.Controls;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas.UI.Xaml;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace OrangeBugReloaded.App
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel MainVM { get; }

        public MainPage()
        {
            InitializeComponent();
            MainVM = new MainViewModel(canvasClient1, canvasClient2, canvasServer);
            DataContext = this;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        private async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.Left:
                    await MainVM.Client1.MovePlayerAsync(Point.West);
                    break;

                case VirtualKey.Right:
                    await MainVM.Client1.MovePlayerAsync(Point.East);
                    break;

                case VirtualKey.Up:
                    await MainVM.Client1.MovePlayerAsync(Point.North);
                    break;

                case VirtualKey.Down:
                    await MainVM.Client1.MovePlayerAsync(Point.South);
                    break;

                case VirtualKey.Escape:
                    await MainVM.Client1.DisconnectAsync();
                    break;

                case VirtualKey.F5:
                    await MainVM.Client1.ConnectAsync(MainVM.Server);
                    MainVM.RendererClient1.Map = MainVM.Client1.Map;
                    break;

                case VirtualKey.A:
                    await MainVM.Client2.MovePlayerAsync(Point.West);
                    break;

                case VirtualKey.D:
                    await MainVM.Client2.MovePlayerAsync(Point.East);
                    break;

                case VirtualKey.W:
                    await MainVM.Client2.MovePlayerAsync(Point.North);
                    break;

                case VirtualKey.S:
                    await MainVM.Client2.MovePlayerAsync(Point.South);
                    break;
            }

            MainVM.RendererClient1.CameraPosition = MainVM.Client1.PlayerPosition.ToVector2();
            MainVM.RendererClient2.CameraPosition = MainVM.Client2.PlayerPosition.ToVector2();

            args.Handled = true;
        }

        private void canvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var canvas = (CanvasAnimatedControl)sender;
            var delta = e.GetCurrentPoint(canvas).Properties.MouseWheelDelta / 200f;
            var normalizedDelta = Mathf.Abs(delta) + 1;

            var renderer =
                (canvas == canvasClient1) ? MainVM.RendererClient1 :
                (canvas == canvasClient2) ? MainVM.RendererClient2 :
                MainVM.RendererServer;

            if (delta >= 0)
                renderer.ZoomLevel = Mathf.Clamp(renderer.ZoomLevel * normalizedDelta, 10, 200);
            else
                renderer.ZoomLevel = Mathf.Clamp(renderer.ZoomLevel / normalizedDelta, 10, 200);
        }

        private async void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var canvas = (CanvasAnimatedControl)sender;
            var currentPoint = e.GetCurrentPoint(canvas);

            if (editToggle.IsOn)
                await MainVM.EditMapAsync(currentPoint.Position.ToVector2());
            else
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

            await MainVM.Client1.MovePlayerAsync(direction);
            MainVM.RendererClient1.CameraPosition = MainVM.Client1.PlayerPosition.ToVector2();
        }
    }
}
