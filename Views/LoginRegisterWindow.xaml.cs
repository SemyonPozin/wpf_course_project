using coach_search.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace coach_search.Views
{
    /// <summary>
    /// Interaction logic for LoginRegister.xaml
    /// </summary>
    public partial class LoginRegisterWindow : Window
    {
        private LoginRegisterViewModel viewModel;
        public LoginRegisterWindow()
        {
            InitializeComponent();
            viewModel = new LoginRegisterViewModel();
            DataContext = viewModel;
        }

        private void LoginPasswordBox_OnChanged(object sender, RoutedEventArgs e)
        {
            viewModel.LoginPassword = ((PasswordBox)sender).Password;
        }

        private void RegPasswordBox_OnChanged(object sender, RoutedEventArgs e)
        {
            viewModel.RegPassword = ((PasswordBox)sender).Password;
        }
    }
}
