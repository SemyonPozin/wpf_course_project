using System.Windows.Controls;
using coach_search.ViewModels;

namespace coach_search.Views
{
    public partial class AdminPanelView : Page
    {
        private AdminPanelViewModel viewModel;

        public AdminPanelView()
        {
            InitializeComponent();
            viewModel = new AdminPanelViewModel();
            DataContext = viewModel;
            Loaded += AdminPanelView_Loaded;
        }

        private async void AdminPanelView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await viewModel.InitializeAsync();
        }
    }
}

