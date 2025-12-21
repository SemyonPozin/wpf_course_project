using coach_search.DB;
using coach_search.Models;
using coach_search.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace coach_search.ViewModels
{
    public class AdminPanelViewModel : BaseViewModel
    {
        private readonly Context _context = new Context();
        private readonly UnitOfWork unitOfWork;

        // Коллекции
        public ObservableCollection<User> Users { get; set; } = new();
        public ObservableCollection<User> Tutors { get; set; } = new();
        public ObservableCollection<Review> Reviews { get; set; } = new();

        // Выбранные элементы
        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }

        private User _selectedTutor;
        public User SelectedTutor
        {
            get => _selectedTutor;
            set
            {
                _selectedTutor = value;
                OnPropertyChanged(nameof(SelectedTutor));
            }
        }

        private Review _selectedReview;
        public Review SelectedReview
        {
            get => _selectedReview;
            set
            {
                _selectedReview = value;
                OnPropertyChanged(nameof(SelectedReview));
            }
        }

        // Команды
        public ICommand LoadDataCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand BlockUserCommand { get; }
        public ICommand UnblockUserCommand { get; }
        public ICommand EditTutorCommand { get; }
        public ICommand BlockTutorCommand { get; }
        public ICommand UnblockTutorCommand { get; }
        public ICommand DeleteReviewCommand { get; }

        public AdminPanelViewModel()
        {
            unitOfWork = new UnitOfWork(_context);
            
            LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
            EditUserCommand = new RelayCommand(obj => EditUser(obj));
            BlockUserCommand = new AsyncRelayCommand(async obj => await BlockUserAsync(obj));
            UnblockUserCommand = new AsyncRelayCommand(async obj => await UnblockUserAsync(obj));
            EditTutorCommand = new RelayCommand(obj => EditTutor(obj));
            BlockTutorCommand = new AsyncRelayCommand(async obj => await BlockTutorAsync(obj));
            UnblockTutorCommand = new AsyncRelayCommand(async obj => await UnblockTutorAsync(obj));
            DeleteReviewCommand = new AsyncRelayCommand(async obj => await DeleteReviewAsync(obj));
        }

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Загружаем пользователей (клиентов)
                Users.Clear();
                var clients = await unitOfWork.Users.GetClientsAsync();
                foreach (var client in clients)
                    Users.Add(client);

                // Загружаем репетиторов
                Tutors.Clear();
                var tutors = await unitOfWork.Users.GetTutorsAsync();
                foreach (var tutor in tutors)
                {
                    // Убеждаемся, что TutorInfo загружен
                    if (tutor.TutorInfo == null)
                    {
                        tutor.TutorInfo = await unitOfWork.Tutors.GetByUserIdAsync(tutor.Id);
                    }
                    Tutors.Add(tutor);
                }

                // Загружаем отзывы
                Reviews.Clear();
                var allReviews = await unitOfWork.Reviews.GetAllReviewsWithIncludesAsync();
                foreach (var review in allReviews)
                    Reviews.Add(review);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditUser(object obj)
        {
            if (obj is User user)
            {
                // Создаем копию для редактирования
                var userCopy = new User
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    IsBlocked = user.IsBlocked,
                    PhotoPath = user.PhotoPath,
                    Role = user.Role
                };

                var editWindow = new AdminEditUserWindow(userCopy);
                if (editWindow.ShowDialog() == true)
                {
                    // Обновляем оригинальный объект
                    user.FullName = userCopy.FullName;
                    user.Email = userCopy.Email;
                    user.Phone = userCopy.Phone;
                    user.IsBlocked = userCopy.IsBlocked;
                    
                    await unitOfWork.Users.UpdateAsync(user);
                    await unitOfWork.SaveAsync();
                    _ = LoadDataAsync();
                }
            }
        }

        private async void EditTutor(object obj)
        {
            if (obj is User tutor)
            {
                if (tutor.TutorInfo == null)
                    tutor.TutorInfo = await unitOfWork.Tutors.GetByUserIdAsync(tutor.Id);

                var editWindow = new AdminEditTutorWindow(tutor); // передаем оригинальный объект
                if (editWindow.ShowDialog() == true)
                {
                    // Сохраняем изменения в БД
                    if (tutor.TutorInfo != null)
                        await unitOfWork.Tutors.UpdateAsync(tutor.TutorInfo);
                    await unitOfWork.Users.UpdateAsync(tutor);
                    await unitOfWork.SaveAsync();

                    _ = LoadDataAsync(); 
                }
            }
        }

        private async Task BlockUserAsync(object obj)
        {
            if (obj is User user)
            {
                if (MessageBox.Show($"Заблокировать пользователя {user.FullName}?", "Подтверждение", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    user.IsBlocked = true;
                    await unitOfWork.Users.UpdateAsync(user);
                    await unitOfWork.SaveAsync();
                    _ = LoadDataAsync();
                }
            }
        }

        private async Task UnblockUserAsync(object obj)
        {
            if (obj is User user)
            {
                if (MessageBox.Show($"Разблокировать пользователя {user.FullName}?", "Подтверждение", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    user.IsBlocked = false;
                    await unitOfWork.Users.UpdateAsync(user);
                    await unitOfWork.SaveAsync();
                    _ = LoadDataAsync();
                }
            }
        }

        private async Task BlockTutorAsync(object obj)
        {
            if (obj is User tutor)
            {
                if (MessageBox.Show($"Заблокировать репетитора {tutor.FullName}?", "Подтверждение", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    tutor.IsBlocked = true;
                    await unitOfWork.Users.UpdateAsync(tutor);
                    await unitOfWork.SaveAsync();
                    _ = LoadDataAsync();
                }
            }
        }

        private async Task UnblockTutorAsync(object obj)
        {
            if (obj is User tutor)
            {
                if (MessageBox.Show($"Разблокировать репетитора {tutor.FullName}?", "Подтверждение", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    tutor.IsBlocked = false;
                    await unitOfWork.Users.UpdateAsync(tutor);
                    await unitOfWork.SaveAsync();
                    _ = LoadDataAsync();
                }
            }
        }

        private async Task DeleteReviewAsync(object obj)
        {
            if (obj is Review review)
            {
                if (MessageBox.Show($"Удалить отзыв от {review.Author?.FullName}?", "Подтверждение", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    await unitOfWork.Reviews.DeleteAsync(review);
                    await unitOfWork.SaveAsync();
                    Reviews.Remove(review);
                }
            }
        }
    }
}

