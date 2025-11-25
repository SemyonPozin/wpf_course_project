using System.Windows.Input;
using coach_search.Models;

namespace coach_search.ViewModels
{
    public class BookingCommentViewModel : BaseViewModel
    {
        private string _commentText;
        public string CommentText
        {
            get => _commentText;
            set
            {
                _commentText = value;
                OnPropertyChanged(nameof(CommentText));
            }
        }

        public ICommand OkCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public BookingCommentViewModel()
        {
            // Команды будут установлены в code-behind для доступа к Window
        }
    }
}

