using System.Windows;
using coach_search.Models;

namespace coach_search.Views
{
    public partial class AdminEditUserWindow : Window
    {
        public User User { get; private set; }

        public AdminEditUserWindow(User user)
        {
            InitializeComponent();
            User = user;
            DataContext = user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

