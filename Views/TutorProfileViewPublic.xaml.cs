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
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace coach_search.Views
{
    /// <summary>
    /// Interaction logic for TutorProfileViewPublic.xaml
    /// </summary>
    public partial class TutorProfileViewPublic : Window
    {
        private int? Id {get; set;}
        public TutorProfileViewPublic(int id)
        {
            Id = id;
            InitializeComponent();
            var vm = new TutorProfileViewPublicViewModel(id);
            DataContext = vm;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (TutorProfileViewPublicViewModel)DataContext;
            // Сначала загружаем данные пользователя
            await vm.LoadDataAsync();
            // Затем инициализируем расписание последовательно, чтобы избежать конфликтов с DbContext
            await vm.InitializeScheduleAsync();
        }
    }
}
