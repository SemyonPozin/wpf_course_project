using System.Windows;
using coach_search.Models;

namespace coach_search.Views
{
    public partial class AdminEditTutorWindow : Window
    {
        public User Tutor { get; private set; }

        public AdminEditTutorWindow(User tutor)
        {
            InitializeComponent();
            Tutor = tutor;
            DataContext = tutor;
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

