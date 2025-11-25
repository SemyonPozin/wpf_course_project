using coach_search.DB;
using coach_search.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Controls;
using coach_search.Views;

namespace coach_search.ViewModels
{
    public class TutorProfileViewPublicViewModel : BaseViewModel
    {
        private readonly UnitOfWork unitOfWork = ApplicationContext.unitofwork;
        private int _userId;

        public User Tutor { get; private set; }
        public TutorInfo TutorInfo { get; private set; }

        // Основная информация
        public string FullName { get; set; }
        public string PhotoPath { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public float PricePerHour { get; set; }
        public string Description { get; set; }

        // Отзывы
        public ObservableCollection<Review> Reviews { get; } = new();

        public string NewReviewText { get; set; }
        public int NewReviewRating { get; set; } = 5;
        public ICommand AddReviewCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ViewUserProfileCommand { get; }
        
        private Visibility _descriptionVisibility = Visibility.Collapsed;
        public Visibility DescriptionVisibility
        {
            get => _descriptionVisibility;
            set
            {
                _descriptionVisibility = value;
                OnPropertyChanged(nameof(DescriptionVisibility));
            }
        }

        private Visibility _reviewsVisibility = Visibility.Collapsed;
        public Visibility ReviewsVisibility
        {
            get => _reviewsVisibility;
            set
            {
                _reviewsVisibility = value;
                OnPropertyChanged(nameof(ReviewsVisibility));
            }
        }

        private object _schedule;
        public object Schedule
        {
            get => _schedule;
            private set
            {
                _schedule = value;
                OnPropertyChanged(nameof(Schedule));
            }
        }

        // Команда создания брони (двойной клик)
        public RelayCommand CreateBookingCommand { get; }

        public TutorProfileViewPublicViewModel(int id)
        {
            _userId = id;
            AddReviewCommand = new RelayCommand(async _ => await AddReview(), _ => !string.IsNullOrWhiteSpace(NewReviewText));
            BackCommand = new RelayCommand(_ => back());
            ViewUserProfileCommand = new RelayCommand(obj => ViewUserProfile(obj));
            var scheduleViewModel = new TutorScheduleViewModel();
            Schedule = new Views.TutorScheduleView(id)
            {
                DataContext = scheduleViewModel
            };
            // Инициализация расписания будет выполнена после загрузки данных в Window_Loaded
        }

        private void back()
        {
            var main = new MainWindow();
            main.Show();
            Application.Current.Windows.OfType<TutorProfileViewPublic>().First().Close();
        }

        private async Task AddReview()
        {
            if (ApplicationContext.CurrentUser == null)
            {
                MessageBox.Show("Для добавления отзыва необходимо войти.");
                return;
            }

            var review = new Review
            {
                TutorId = Tutor.Id,
                AuthorId = ApplicationContext.CurrentUser.Id,
                Rating = NewReviewRating,
                Text = NewReviewText,
                CreatedAt = DateTime.Now
            };

            await unitOfWork.Reviews.AddAsync(review);
            await unitOfWork.SaveAsync();

            // Добавляем в ObservableCollection, чтобы UI обновился
            Reviews.Add(review);
            ReviewsVisibility = Visibility.Visible;

            // Очистка полей
            NewReviewText = string.Empty;
            NewReviewRating = 5;
            OnPropertyChanged(nameof(NewReviewText));
            OnPropertyChanged(nameof(NewReviewRating));
        }
        // -----------------------------
        // Загрузка данных
        // -----------------------------
        public async Task LoadData(int UserId)
        {
            Tutor = await unitOfWork.Users.GetByIdAsync(UserId);
            if (Tutor == null)
            {
                MessageBox.Show("Репетитор не найден.");
                return;
            }

            TutorInfo = await unitOfWork.Tutors.GetByUserIdAsync(UserId);
            if (TutorInfo == null)
            {
                MessageBox.Show("Информация о репетиторе не найдена.");
                return;
            }

            FullName = Tutor.FullName;
            PhotoPath = Tutor.PhotoPath;
            Phone = Tutor.Phone;
            Email = Tutor.Email;

            Subject = TutorInfo.Subject;
            PricePerHour = TutorInfo.PricePerHour;
            Description = TutorInfo.Description;

            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(PhotoPath));
            OnPropertyChanged(nameof(Phone));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Subject));
            OnPropertyChanged(nameof(PricePerHour));
            OnPropertyChanged(nameof(Description));
            DescriptionVisibility = string.IsNullOrWhiteSpace(Description) ? Visibility.Collapsed : Visibility.Visible;

            await LoadReviews();
        }

        public async Task LoadDataAsync()
        {
            int userId = _userId;
            Tutor = await unitOfWork.Users.GetByIdAsync(userId);
            if (Tutor == null) return;

            TutorInfo = await unitOfWork.Tutors.GetByUserIdAsync(userId);
            if (TutorInfo == null) return;

            FullName = Tutor.FullName;
            PhotoPath = Tutor.PhotoPath;
            Phone = Tutor.Phone;
            Email = Tutor.Email;
            Subject = TutorInfo.Subject;
            PricePerHour = TutorInfo.PricePerHour;
            Description = TutorInfo.Description;

            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(PhotoPath));
            OnPropertyChanged(nameof(Phone));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Subject));
            OnPropertyChanged(nameof(PricePerHour));
            OnPropertyChanged(nameof(Description));
            DescriptionVisibility = string.IsNullOrWhiteSpace(Description) ? Visibility.Collapsed : Visibility.Visible;

            await LoadReviewsAsync();
        }


        // -----------------------------
        // Загрузка отзывов
        // -----------------------------
        private async Task LoadReviews()
        {
            Reviews.Clear();
            var list = await unitOfWork.Reviews.GetReviewsForTutorAsync(Tutor.Id);

            foreach (var r in list)
                Reviews.Add(r);
            
            ReviewsVisibility = Reviews.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadReviewsAsync()
        {
            Reviews.Clear();
            var list = await unitOfWork.Reviews.GetReviewsForTutorAsync(Tutor.Id);
            foreach (var r in list)
                Reviews.Add(r);
            
            ReviewsVisibility = Reviews.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public async Task InitializeScheduleAsync()
        {
            if (Schedule is Views.TutorScheduleView scheduleView && scheduleView.DataContext is TutorScheduleViewModel tsvm)
                await tsvm.InitializeAsync(_userId);
        }

        private void ViewUserProfile(object userObj)
        {
            if (userObj is User user)
            {
                var profileWindow = new Views.UserProfileWindow(user);
                profileWindow.ShowDialog();
            }
        }

    }
}