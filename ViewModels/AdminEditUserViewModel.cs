using coach_search.Models;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace coach_search.ViewModels
{
    public class AdminEditUserViewModel : BaseViewModel
    {
        private readonly Window _window;
        private readonly User _user;

        public AdminEditUserViewModel(User user, Window window)
        {
            _user = user;
            _window = window;

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());

            // Инициализация
            FullName = user.FullName;
            Email = user.Email;
            Phone = user.Phone;
            IsBlocked = user.IsBlocked;
        }

        #region Properties

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(nameof(FullName)); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(nameof(Phone)); }
        }

        private bool _isBlocked;
        public bool IsBlocked
        {
            get => _isBlocked;
            set { _isBlocked = value; OnPropertyChanged(nameof(IsBlocked)); }
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Email) ||
            !Regex.IsMatch(Email,
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]\.[a-zA-Z]{2,6}$"))
            {
                MessageBox.Show("Введите корректный email.\nПример: example@gmail.com",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(FullName) || FullName.Length < 5 || FullName.Length > 50)
            {
                MessageBox.Show("Введите корректное ФИО (минимум 5 и максимум 50 символов).",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Phone) ||
                !Regex.IsMatch(Phone, @"^\+375(29|25|33|44)\d{7}$"))
            {
                MessageBox.Show("Введите корректный номер телефона.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // записываем изменения обратно в модель
            _user.FullName = FullName;
            _user.Email = Email;
            _user.Phone = Phone;
            _user.IsBlocked = IsBlocked;

            _window.DialogResult = true;
            _window.Close();
        }

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}
