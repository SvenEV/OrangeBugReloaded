using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.ClientServer;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace OrangeBugReloaded.App.Presentation
{
    public sealed partial class OrangeBugPresenter : UserControl
    {
        private readonly OrangeBugRenderer _renderer = new OrangeBugRenderer();
        private GameClientBase _gameClient;
        private float _initialZoomLevel;
        private float _initialCameraPosition;

        public IGameplayMap Map
        {
            get { return _renderer.Map; }
            set { _renderer.Map = value; }
        }

        public GameClientBase GameClient
        {
            get { return _gameClient; }
            set
            {
                if (!Equals(_gameClient, value))
                {
                    _gameClient = value;
                    _renderer.FollowedPlayerId = value?.PlayerInfo.PlayerId;
                }
            }
        }

        public OrangeBugPresenter()
        {
            InitializeComponent();
            _renderer.Canvas = canvas;
            KeyboardManager.ArrowKeyDown.Subscribe(OnDirectionKeyDown);
        }

        private void OnPointerWheelChanged(object _, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(canvas).Properties.MouseWheelDelta / 200f;
            var normalizedDelta = Mathf.Abs(delta) + 1;

            if (delta >= 0)
                _renderer.ZoomLevel = Mathf.Clamp(_renderer.ZoomLevel * normalizedDelta, .1f, 5);
            else
                _renderer.ZoomLevel = Mathf.Clamp(_renderer.ZoomLevel / normalizedDelta, .1f, 5);
        }

        private async void OnDirectionKeyDown(Point direction)
        {
            if (GameClient != null)
                await GameClient.MovePlayerAsync(direction);
        }

        // TODO: Use a combination of PointerPressed/PointerReleased instead of tap events
        private async void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (GameClient == null)
                return;

            var position = e.GetPosition(canvas);
            await PerformBasicTouchNavigation(canvas, position.ToVector2());
        }

        private async void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (GameClient == null)
                return;

            var position = e.GetPosition(canvas);
            await PerformBasicTouchNavigation(canvas, position.ToVector2());
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            _renderer.ZoomLevel *= e.Delta.Scale;
            _renderer.CameraPosition += new Vector2(
                (float)-e.Delta.Translation.X / (_renderer.ZoomLevel * 200),
                (float)e.Delta.Translation.Y / (_renderer.ZoomLevel * 200));

            _renderer.CurrentZoomLevel = _renderer.ZoomLevel;
            _renderer.CurrentCameraPosition = _renderer.CameraPosition;
        }

        private async Task PerformBasicTouchNavigation(CanvasAnimatedControl canvas, Vector2 position)
        {
            var x = position.X / canvas.Size.Width * 2 - 1;
            var y = position.Y / canvas.Size.Height * 2 - 1;
            Point direction;

            if (Math.Abs(x) < Math.Abs(y))
                direction = y < 0 ? Point.North : Point.South;
            else
                direction = x < 0 ? Point.West : Point.East;

            await GameClient.MovePlayerAsync(direction);
        }
    }
}
