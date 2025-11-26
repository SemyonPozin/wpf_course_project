using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using coach_search.DB;
using coach_search.Models;
using System.Windows.Controls;
using coach_search.Views;


namespace coach_search.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public NavigationService Navigation { get; }

        public object CurrentPage => Navigation.CurrentPage;

        public ICommand NavigateHomeCommand { get; }
        public ICommand NavigateProfileCommand { get; }
        public ICommand NavigateAdminCommand { get; }
        //public ICommand NavigateTutorCommand { get; }
        public ICommand LogoutCommand { get; }

        // Видимость кнопок
        public Visibility AdminButtonVisibility =>
            ApplicationContext.CurrentUser?.Role == 2 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ProfileButtonVisibility =>
            ApplicationContext.CurrentUser?.Role != 2 ? Visibility.Visible : Visibility.Collapsed;

        public MainWindowViewModel()
        {
            Navigation = new NavigationService();
            Navigation.PropertyChanged += (_, __) => OnPropertyChanged(nameof(CurrentPage));

            NavigateHomeCommand = new RelayCommand(_ => Navigation.Navigate(new HomePageViewModel()));
            NavigateProfileCommand = new RelayCommand(_ => ChooseProfileWindow());
            //NavigateTutorCommand = new RelayCommand((object id) => Navigation.Navigate(new TutorProfileViewPublicViewModel((int)id)));
            NavigateAdminCommand = new RelayCommand(_ => Navigation.Navigate(new AdminPanelViewModel()));
            LogoutCommand = new RelayCommand(_ => Logout());

            // По умолчанию открываем главную
            Navigation.Navigate(new HomePageViewModel());
        }

        private void ChooseProfileWindow()
        {
            if(ApplicationContext.CurrentUser?.Role == 1) 
                Navigation.Navigate(new TutorProfileViewModel());
            else 
                Navigation.Navigate(new ClientProfileViewModel());
        }

        private void Logout()
        {
            ApplicationContext.CurrentUser = null;
            var log = new LoginRegisterWindow();
            log.Show();
            Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
        }
    }



}


