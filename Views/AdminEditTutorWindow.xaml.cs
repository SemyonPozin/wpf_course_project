using System.Windows;
using coach_search.Models;
using coach_search.ViewModels;

namespace coach_search.Views
{
    public partial class AdminEditTutorWindow : Window
    {
        public AdminEditTutorWindow(User tutor)
        {
            InitializeComponent();
            DataContext = new AdminEditTutorViewModel(tutor, this);
        }
    }
}
