using matchmaking.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace matchmaking.Views
{
    internal sealed partial class AgeBlockView : Page
    {
        internal SplashViewModel? ViewModel { get; private set; }

        public AgeBlockView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is SplashViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }

        private void HandleExitAppClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}