using System.Windows;
using coach_search.ViewModels;
using System.Windows.Input;

namespace coach_search.Views
{
    /// <summary>
    /// Interaction logic for BookingCommentWindow.xaml
    /// </summary>
    public partial class BookingCommentWindow : Window
    {
        private BookingCommentViewModel _viewModel;
        
        public string CommentText => _viewModel?.CommentText;

        public BookingCommentWindow()
        {
            InitializeComponent();
            _viewModel = new BookingCommentViewModel();
            DataContext = _viewModel;
            
            // Устанавливаем команды для закрытия окна
            _viewModel.OkCommand = new RelayCommand(_ => 
            {
                DialogResult = true;
                Close();
            });
            _viewModel.CancelCommand = new RelayCommand(_ => 
            {
                _viewModel.CommentText = null;
                DialogResult = false;
                Close();
            });
        }
    }
}
