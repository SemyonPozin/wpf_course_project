using System.Windows;
using coach_search.Models;

namespace coach_search.Views
{
    public partial class UserProfileWindow : Window
    {
        public UserProfileWindow(User user)
        {
            InitializeComponent();
            DataContext = user;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

