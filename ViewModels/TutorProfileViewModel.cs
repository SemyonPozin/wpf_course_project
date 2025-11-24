using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using coach_search.DB.coach_search.DB;
using coach_search.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Windows;
using System.Collections.ObjectModel;
using coach_search.Views;

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

        public bool IsTutor => User?.Role == 1;

        public Visibility ReviewsVisibility =>
            Reviews.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        // -----------------------
        // Commands
        // -----------------------
        public RelayCommand EditCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand UploadPhotoCommand { get; }

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(nameof(IsEditing)); }
        }
        private bool _isEditing;

        private TutorInfo _tutorInfoCache;

        public TutorProfileViewModel()
        {
            User = ApplicationContext.CurrentUser;

            LoadUser();

            EditCommand = new RelayCommand(_ => BeginEdit(), _ => !IsEditing);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => IsEditing);
            CancelCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);
            UploadPhotoCommand = new RelayCommand(_ => UploadPhoto(), _ => IsEditing);

            Schedule = new TutorScheduleView(ApplicationContext.CurrentUser?.Id);
        }

        private async Task LoadUser()
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

        }

        private void BeginEdit() => IsEditing = true;

        private void CancelEdit()
        {
            IsEditing = false;
            LoadUser();
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

            if (string.IsNullOrWhiteSpace(Subject) || !Regex.IsMatch(Subject, @"^[A-Za-zА-Яа-яЁё]{3,}$"))
            {
                MessageBox.Show("Введите корректный предмет.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            // Full name
            if (string.IsNullOrWhiteSpace(FullName) || FullName.Length < 5)
            {
                MessageBox.Show("Введите корректное ФИО (минимум 5 символов).",
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
