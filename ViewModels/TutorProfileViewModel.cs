using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using coach_search.DB;
using coach_search.Models;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Windows;
using System.Collections.ObjectModel;
using coach_search.Views;
using System.Windows.Input;

namespace coach_search.ViewModels
{
    public class TutorProfileViewModel : BaseViewModel
    {
        private readonly UnitOfWork unitOfWork = ApplicationContext.unitofwork;

        public User? User { get; private set; }

        // -----------------------
        // Editable properties
        // -----------------------

        public string PhotoPath
        {
            get => _photoPath;
            set { _photoPath = value; OnPropertyChanged(nameof(PhotoPath)); }
        }
        private string _photoPath;

        public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(nameof(FullName)); } }
        private string _fullName;

        public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(nameof(Phone)); } }
        private string _phone;

        public string Email { get => _email; set { _email = value; OnPropertyChanged(nameof(Email)); } }
        private string _email;

        public string Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }
        private string _description;

        public string Subject { get => _subject; set { _subject = value; OnPropertyChanged(nameof(Subject)); } }
        private string _subject;

        public float PricePerHour { get => _pricePerHour; set { _pricePerHour = value; OnPropertyChanged(nameof(PricePerHour)); } }
        private float _pricePerHour;

        public ObservableCollection<Review> Reviews { get; set; } = new();
        public ObservableCollection<Appointment> PendingAppointments { get; set; } = new();

        public bool IsTutor => User?.Role == 1;

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
        
        private Visibility _pendingAppointmentsVisibility = Visibility.Collapsed;
        public Visibility PendingAppointmentsVisibility
        {
            get => _pendingAppointmentsVisibility;
            set
            {
                _pendingAppointmentsVisibility = value;
                OnPropertyChanged(nameof(PendingAppointmentsVisibility));
            }
        }

        // -----------------------
        // Commands
        // -----------------------
        public RelayCommand EditCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand UploadPhotoCommand { get; }
        public ICommand AcceptAppointmentCommand { get; }
        public ICommand RejectAppointmentCommand { get; }

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(nameof(IsEditing)); }
        }
        private bool _isEditing;

        private TutorInfo _tutorInfoCache;
        private TutorScheduleViewModel _scheduleViewModel;

        public TutorProfileViewModel()
        {
            User = ApplicationContext.CurrentUser;

            EditCommand = new RelayCommand(_ => BeginEdit(), _ => !IsEditing);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => IsEditing);
            CancelCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);
            UploadPhotoCommand = new RelayCommand(_ => UploadPhoto(), _ => IsEditing);
            AcceptAppointmentCommand = new AsyncRelayCommand(async obj => await AcceptAppointmentAsync(obj));
            RejectAppointmentCommand = new AsyncRelayCommand(async obj => await RejectAppointmentAsync(obj));

            // Создаем ViewModel для расписания
            _scheduleViewModel = new TutorScheduleViewModel();
            Schedule = new TutorScheduleView
            {
                DataContext = _scheduleViewModel
            };
        }

        public async Task InitializeAsync()
        {   
            // Затем загружаем расписание - используем User.Id, как в TutorProfileViewPublic используется _userId
            if (User != null && User.Id > 0)
                await _scheduleViewModel.LoadScheduleAsync(User.Id);
            else
                _scheduleViewModel.IsLoading = false;
            
        }

        public async Task LoadUser()
        {
            FullName = User.FullName;
            Phone = User.Phone;
            Email = User.Email;
            PhotoPath = User.PhotoPath;

            _tutorInfoCache = await unitOfWork.Tutors.GetByUserIdAsync(User.Id);

            if (_tutorInfoCache != null)
            {
                Description = _tutorInfoCache.Description;
                Subject = _tutorInfoCache.Subject;
                PricePerHour = _tutorInfoCache.PricePerHour;
            }

            // загрузка отзывов
            Reviews.Clear();
            var rev = await unitOfWork.Reviews.GetReviewsForTutorAsync(User.Id);
            foreach (var r in rev)
                Reviews.Add(r);
            ReviewsVisibility = Reviews.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            // загрузка заявок со статусом pending
            await LoadPendingAppointments();
        }

        private async Task LoadPendingAppointments()
        {
            PendingAppointments.Clear();
            var appointments = await unitOfWork.Appointments.GetTutorAppointmentsAsync(User.Id);
            var pending = appointments.Where(a => a.Status == 0).ToList();
            foreach (var a in pending)
                PendingAppointments.Add(a);
            PendingAppointmentsVisibility = PendingAppointments.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task AcceptAppointmentAsync(object appointmentObj)
        {
            if (appointmentObj is Appointment appointment)
            {
                appointment.Status = 1; // accepted
                await unitOfWork.Appointments.UpdateAsync(appointment);
                await unitOfWork.SaveAsync();
                
                PendingAppointments.Remove(appointment);
                PendingAppointmentsVisibility = PendingAppointments.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                
                // Обновляем расписание
                if (User?.Id != null)
                {
                    await _scheduleViewModel.LoadScheduleAsync(User.Id);
                }
            }
        }

        private async Task RejectAppointmentAsync(object appointmentObj)
        {
            if (appointmentObj is Appointment appointment)
            {
                appointment.Status = 2; // rejected
                await unitOfWork.Appointments.UpdateAsync(appointment);
                await unitOfWork.SaveAsync();
                
                PendingAppointments.Remove(appointment);
                PendingAppointmentsVisibility = PendingAppointments.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                
                // Обновляем расписание
                if (User?.Id != null)
                {
                    await _scheduleViewModel.LoadScheduleAsync(User.Id);
                }
            }
        }

        private void BeginEdit() => IsEditing = true;

        private void CancelEdit()
        {
            IsEditing = false;
            _ = LoadUser();
        }

        // -------------------------
        // Upload photo
        // -------------------------
        private void UploadPhoto()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dialog.ShowDialog() == true)
            {
                PhotoPath = dialog.FileName;
            }
        }

        // -------------------------
        // Save with validation
        // -------------------------
        private async Task SaveAsync()
        {
            // Email
            if (string.IsNullOrWhiteSpace(Email) ||
                !Regex.IsMatch(Email, 
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]\.[a-zA-Z]{2,6}$"))
            {
                MessageBox.Show("Введите корректный email.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (await unitOfWork.Users.GetByEmailAsync(Email) != null && ApplicationContext.CurrentUser.Email != Email)
            {
                MessageBox.Show("Этот email уже зарегистрирован.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Subject) || !Regex.IsMatch(Subject, @"^[A-Za-zА-Яа-яЁё]{3,}$"))
            {
                MessageBox.Show("Введите корректный предмет.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Full name
            if (string.IsNullOrWhiteSpace(FullName) || FullName.Length < 5 || FullName.Length > 50)
            {
                MessageBox.Show("Введите корректное ФИО (минимум 5 и максимум 50 символов).",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Phone
            if (string.IsNullOrWhiteSpace(Phone) ||
                !Regex.IsMatch(Phone, @"^\+375(29|33|44)\d{7}$"))
            {
                MessageBox.Show("Введите корректный номер телефона.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PricePerHour <= 0)
            {
                MessageBox.Show("Цена за час должна быть больше 0.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(Description.Length > 300)
            {
                MessageBox.Show("Длина описания не должна быть более 300 символов.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Subject.Length > 15)
            {
                MessageBox.Show("Длина названия предмета не должна быть более 15 символов.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --------------------
            // Save user
            // --------------------
            User.FullName = FullName;
            User.Email = Email;
            User.Phone = Phone;
            User.PhotoPath = PhotoPath;

            await unitOfWork.Users.UpdateAsync(User);

            // --------------------
            // Save tutor info
            // --------------------
            _tutorInfoCache.Description = Description;
            _tutorInfoCache.Subject = Subject;
            _tutorInfoCache.PricePerHour = PricePerHour;
            await unitOfWork.Tutors.UpdateAsync(_tutorInfoCache);

            await unitOfWork.SaveAsync();

            IsEditing = false;
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
    }
}
