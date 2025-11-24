using coach_search.DB.coach_search.DB;
using coach_search.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace coach_search.ViewModels
{
    public class ClientProfileViewModel : BaseViewModel
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

        public ObservableCollection<Review> Reviews { get; set; } = new();

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

        public ClientProfileViewModel()
        {
            User = ApplicationContext.CurrentUser;

            LoadUser();

            EditCommand = new RelayCommand(_ => BeginEdit(), _ => !IsEditing);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => IsEditing);
            CancelCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);
            UploadPhotoCommand = new RelayCommand(_ => UploadPhoto(), _ => IsEditing);
        }

        private async Task LoadUser()
        {
            FullName = User.FullName;
            Phone = User.Phone;
            Email = User.Email;
            PhotoPath = User.PhotoPath;

            // загрузка отзывов
            Reviews.Clear();
            var rev = await unitOfWork.Reviews.GetReviewsByUserIdAsync(User.Id);
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

            // --------------------
            // Save user
            // --------------------
            User.FullName = FullName;
            User.Email = Email;
            User.Phone = Phone;
            User.PhotoPath = PhotoPath;

            await unitOfWork.Users.UpdateAsync(User);

            await unitOfWork.SaveAsync();

            IsEditing = false;
        }
    }
}

