using OrangeBugReloaded.Core;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace OrangeBugReloaded.App.Presentation
{
    public static class KeyboardManager
    {
        private static bool _isInitialized = false;
        private static readonly Subject<KeyDownEvent> _keyDown = new Subject<KeyDownEvent>();

        public static IObservable<KeyDownEvent> KeyDown => _keyDown;

        public static IObservable<Point> ArrowKeyDown => _keyDown.Select(ToDirection).Where(p => p.IsDirection);

        public static void Initialize()
        {
            if (_isInitialized)
                return;

            Window.Current.CoreWindow.KeyDown += OnKeyDown;
            _isInitialized = true;
        }

        private static void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            _keyDown.OnNext(new KeyDownEvent(args.VirtualKey, args.KeyStatus));
        }

        private static Point ToDirection(KeyDownEvent e)
        {
            return
                e.Key == VirtualKey.Left ? Point.West :
                e.Key == VirtualKey.Right ? Point.East :
                e.Key == VirtualKey.Up ? Point.North :
                e.Key == VirtualKey.Down ? Point.South :
                Point.Zero;
        }
    }

    public class KeyDownEvent
    {
        public VirtualKey Key { get; }
        public CorePhysicalKeyStatus KeyStatus { get; }

        public KeyDownEvent(VirtualKey key, CorePhysicalKeyStatus keyStatus)
        {
            Key = key;
            KeyStatus = keyStatus;
        }
    }
}
