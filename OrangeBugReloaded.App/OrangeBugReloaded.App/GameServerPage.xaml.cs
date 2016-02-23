using OrangeBugReloaded.App.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace OrangeBugReloaded.App
{
    public sealed partial class GameServerPage : Page
    {
        public GameServerViewModel ViewModel { get; private set; }

        public GameServerPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var port = e.Parameter.ToString();
            ViewModel = new GameServerViewModel(canvas, port);
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
    }
}
