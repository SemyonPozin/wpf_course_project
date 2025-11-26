using coach_search.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace coach_search.Views
{
    public partial class TutorProfileView : Page
    {
        private TutorProfileViewModel vm;
        public TutorProfileView()
        {
            InitializeComponent();
            DataContext = vm = new TutorProfileViewModel();
        }

        public async void Page_loaded(object sender, EventArgs e)
        {
            // Загружаем данные асинхронно - как в TutorProfileViewPublic
            await vm.InitializeAsync();
            await vm.LoadUser();
        }
    }
}
