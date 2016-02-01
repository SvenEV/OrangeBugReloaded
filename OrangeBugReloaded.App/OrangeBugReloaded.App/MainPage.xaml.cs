using OrangeBugReloaded.Core;
using OrangeBugReloaded.App.Common;
using System;
using Windows.UI.Xaml.Controls;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.System;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace OrangeBugReloaded.App
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Map _map;
        private OrangeBugRenderer _renderer;
        private Point _playerPosition = new Point(1, 2);

        public MainPage()
        {
            InitializeComponent();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Init();
        }

        private async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.Left:
                    if (await _map.MoveAsync(_playerPosition, _playerPosition + Point.West))
                        _playerPosition += Point.West;
                    break;

                case VirtualKey.Right:
                    if (await _map.MoveAsync(_playerPosition, _playerPosition + Point.East))
                        _playerPosition += Point.East;
                    break;

                case VirtualKey.Up:
                    if (await _map.MoveAsync(_playerPosition, _playerPosition + Point.North))
                        _playerPosition += Point.North;
                    break;

                case VirtualKey.Down:
                    if (await _map.MoveAsync(_playerPosition, _playerPosition + Point.South))
                        _playerPosition += Point.South;
                    break;
            }

            _renderer.CameraPosition = new Vector2(_playerPosition.X, _playerPosition.Y);
        }

        private async void Init()
        {
            _map = new Map(new InMemoryChunkStorage(new[] { Chunk.SampleChunk, Chunk.SampleChunk2 }));

            _renderer = new OrangeBugRenderer();
            _renderer.Attach(canvas);
            _renderer.Plugins.Add<OrangeBugAudioPlayer>();
            _renderer.Plugins.Add<EntityMoveAnimationPlugin>();
            _renderer.Map = _map;

            await _map.ChunkLoader.TryGetAsync(Point.Zero);
        }

        private void canvas_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(canvas).Properties.MouseWheelDelta / 200f;
            var normalizedDelta = Mathf.Abs(delta) + 1;

            if (delta >= 0)
                _renderer.ZoomLevel = Mathf.Clamp(_renderer.ZoomLevel * normalizedDelta, 10, 200);
            else
            {
                var change =
                _renderer.ZoomLevel = Mathf.Clamp(_renderer.ZoomLevel / normalizedDelta, 10, 200);
            }
        }

        private async void canvas_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var pos = e.GetCurrentPoint(canvas).Position;

            var x = pos.X / canvas.Size.Width * 2 - 1;
            var y = pos.Y / canvas.Size.Height * 2 - 1;
            Point direction;

            if (Math.Abs(x) < Math.Abs(y))
            {
                direction = y < 0 ? Point.North : Point.South;
            }
            else
            {
                direction = x < 0 ? Point.West : Point.East;
            }

            if (await _map.MoveAsync(_playerPosition, _playerPosition + direction))
                _playerPosition += direction;

            _renderer.CameraPosition = new Vector2(_playerPosition.X, _playerPosition.Y);
        }
    }
}
