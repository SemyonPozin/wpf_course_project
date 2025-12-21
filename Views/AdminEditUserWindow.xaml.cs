using System.Windows;
using coach_search.Models;
using coach_search.ViewModels;

namespace coach_search.Views
{
    public partial class AdminEditUserWindow : Window
    {
        public AdminEditUserWindow(User user)
        {
            InitializeComponent();
            DataContext = new AdminEditUserViewModel(user, this);
        }
    }
}
