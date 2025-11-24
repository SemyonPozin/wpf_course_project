using coach_search.DB.coach_search.DB;
using coach_search.Models;
using coach_search.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace coach_search.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        //private readonly NavigationService _navigation;

        private List<User> _allTutors;
        public ObservableCollection<User> FilteredTutors { get; set; } = new();

        public ObservableCollection<string> Subjects { get; set; }
        public string SearchText { get; set; }
        public string SelectedSubject { get; set; } = "Все";
        public string MinPrice { get; set; } = "0";
        public string MaxPrice { get; set; } = "100";

        public ICommand SearchCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand SelectCommand { get; }

        private readonly UnitOfWork _unitOfWork = ApplicationContext.unitofwork;

        public HomePageViewModel()//NavigationService navigation
        {
            //_navigation = navigation;

            Subjects = new ObservableCollection<string>
            {
                "Физика", "Математика", "Руский язык", "Белорусский язык",
                "Химия", "Биология", "География", "История", "Музыка", "Все"
            };

            SearchCommand = new RelayCommand(_ => ExecuteSearch());
            ClearCommand = new RelayCommand(_ => ClearFilters());
            SelectCommand = new RelayCommand((object id) =>
            {
                if (id is int tutorId)
                {
                    ApplicationContext.CurrentTutorId = tutorId;
                    var vm = new TutorProfileViewPublic((int)id);
                    vm.Show();
                    var main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    main.Close();
                    //await vm.LoadData(tutorId);
                    //_navigation.Navigate(vm); // NavigationService остаётся как есть
                }
            });


            _ = LoadTutorsAsync();
        }


        private async Task LoadTutorsAsync()
        {
            _allTutors = await _unitOfWork.Users.GetTutorsAsync();
            FilteredTutors = new ObservableCollection<User>(_allTutors);
            OnPropertyChanged(nameof(FilteredTutors));
        }

        private void ExecuteSearch()
        {
            PriceValidation();

            var filtered = _allTutors.Where(t =>
                (string.IsNullOrWhiteSpace(SearchText) || t.FullName.ToLower().Contains(SearchText?.ToLower() ?? "")) &&
                (SelectedSubject == "Все" || t.TutorInfo.Subject == SelectedSubject) &&
                t.TutorInfo.PricePerHour >= Convert.ToSingle(MinPrice) &&
                t.TutorInfo.PricePerHour <= Convert.ToSingle(MaxPrice)
            );

            FilteredTutors = new ObservableCollection<User>(filtered);
            OnPropertyChanged(nameof(FilteredTutors));
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedSubject = "Все";
            MinPrice = "0";
            MaxPrice = "100";
            ExecuteSearch();
        }

        private void PriceValidation()
        {
            if (!float.TryParse(MinPrice, out float min)) min = 0;
            if (!float.TryParse(MaxPrice, out float max)) max = 100;

            if (max <= min)
            {
                MinPrice = "0";
                MaxPrice = "100";
            }
        }
    }
}
