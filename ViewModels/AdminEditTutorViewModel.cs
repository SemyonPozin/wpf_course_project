using coach_search.DB;
using coach_search.Models;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
namespace coach_search.ViewModels
{
    public class AdminEditTutorViewModel : BaseViewModel
    {
        private readonly Window _window;
        private readonly User _tutor;
    public AdminEditTutorViewModel(User tutor, Window window)
        {
            _tutor = tutor;
            _window = window;

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());

            // Инициализация полей
            FullName = tutor.FullName;
            Email = tutor.Email;
            Phone = tutor.Phone;
            IsBlocked = tutor.IsBlocked;

            TutorInfo = new TutorInfo
            {
                Description = tutor.TutorInfo?.Description,
                Subject = tutor.TutorInfo?.Subject,
                PricePerHour = tutor.TutorInfo?.PricePerHour ?? 0
            };
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

        private TutorInfo _tutorInfo;
        public TutorInfo TutorInfo
        {
            get => _tutorInfo;
            set { _tutorInfo = value; OnPropertyChanged(nameof(TutorInfo)); }
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
                !Regex.IsMatch(Phone, @"^\+375(29|33|44|25)\d{7}$"))
            {
                MessageBox.Show("Введите корректный номер телефона.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_tutor.TutorInfo == null)
                _tutor.TutorInfo = new TutorInfo();

            if (TutorInfo?.Description?.Length > 300)
            {
                MessageBox.Show("Длина описания не должна быть более 300 символов.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TutorInfo?.Subject?.Length > 15)
            {
                MessageBox.Show("Длина названия предмета не должна быть более 15 символов.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Сохраняем данные обратно в модель
            _tutor.FullName = FullName;
            _tutor.Email = Email;
            _tutor.Phone = Phone;
            _tutor.IsBlocked = IsBlocked;

            _tutor.TutorInfo.Description = TutorInfo.Description;
            _tutor.TutorInfo.Subject = TutorInfo.Subject;
            _tutor.TutorInfo.PricePerHour = TutorInfo.PricePerHour;

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