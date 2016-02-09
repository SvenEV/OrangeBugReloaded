using OrangeBugReloaded.Core;
using OrangeBugReloaded.App.Common;
using System;
using Windows.UI.Xaml.Controls;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace OrangeBugReloaded.App
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Point _playerPosition = new Point(1, 2);

        public MainViewModel MainVM { get; }

        public MainPage()
        {
            InitializeComponent();
            MainVM = new MainViewModel(canvas);
            DataContext = this;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        private async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.Left:
                    if (await MainVM.Map.MoveAsync(_playerPosition, _playerPosition + Point.West))
                        _playerPosition += Point.West;
                    break;

                case VirtualKey.Right:
                    if (await MainVM.Map.MoveAsync(_playerPosition, _playerPosition + Point.East))
                        _playerPosition += Point.East;
                    break;

                case VirtualKey.Up:
                    if (await MainVM.Map.MoveAsync(_playerPosition, _playerPosition + Point.North))
                        _playerPosition += Point.North;
                    break;

                case VirtualKey.Down:
                    if (await MainVM.Map.MoveAsync(_playerPosition, _playerPosition + Point.South))
                        _playerPosition += Point.South;
                    break;
            }

            MainVM.Renderer.CameraPosition = new Vector2(_playerPosition.X, _playerPosition.Y);
        }

        private void canvas_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(canvas).Properties.MouseWheelDelta / 200f;
            var normalizedDelta = Mathf.Abs(delta) + 1;

            if (delta >= 0)
                MainVM.Renderer.ZoomLevel = Mathf.Clamp(MainVM.Renderer.ZoomLevel * normalizedDelta, 10, 200);
            else
            {
                var change =
                MainVM.Renderer.ZoomLevel = Mathf.Clamp(MainVM.Renderer.ZoomLevel / normalizedDelta, 10, 200);
            }
        }

        private async void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(canvas);

            if (currentPoint.Properties.IsRightButtonPressed)
                await MainVM.EditMapAsync(currentPoint.Position.ToVector2());
            else
                await PerformBasicTouchNavigation(currentPoint.Position.ToVector2());
        }

        private async Task PerformBasicTouchNavigation(Vector2 position)
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

            if (await MainVM.Map.MoveAsync(_playerPosition, _playerPosition + direction))
                _playerPosition += direction;

            MainVM.Renderer.CameraPosition = new Vector2(_playerPosition.X, _playerPosition.Y);
        }
    }
}
