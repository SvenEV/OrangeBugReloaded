using OrangeBugReloaded.App.ViewModels;
using Windows.UI.Xaml.Controls;

namespace OrangeBugReloaded.App
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = this;
        }
    }
}
