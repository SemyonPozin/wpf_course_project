using coach_search.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using coach_search.Models;

namespace coach_search.Views
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private coach_search.Models.NavigationService Navigation;
        public HomePage()
        {
            //Navigation = new();
            //Navigation.PropertyChanged += (_, __) => OnPropertyChanged(nameof(CurrentPage));
            InitializeComponent();
            DataContext = new HomePageViewModel();//Navigation
        }

        //private void ViewProfile_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Button btn && btn.Tag is int tutorId)
        //    {
        //        // создаём страницу напрямую с параметром
        //        var page = new TutorProfileViewPublic(tutorId);

        //        // ищем Frame на MainWindow и показываем страницу
        //        var mainWindow = Application.Current.Windows.OfType<MainWindow>() as MainWindow;
        //        if (mainWindow != null)
        //        {
        //            mainWindow.MainFrame.Content = page; // MainFrame - x:Name вашего Frame в MainWindow
        //        }
        //    }
        //}
    }
}
