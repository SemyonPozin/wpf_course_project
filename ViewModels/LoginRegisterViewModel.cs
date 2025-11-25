using coach_search.DB;
using coach_search.Models;
using coach_search.ViewModels;
using coach_search.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;


public class LoginRegisterViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public UnitOfWork unitOfWork = ApplicationContext.unitofwork;


    // ===== LOGIN =====
    public string LoginEmail { get; set; }
    public string LoginPassword { get; set; }
    public string LoginError { get; set; }

    // ===== REGISTER =====
    public string RegFullName { get; set; }
    public string RegEmail { get; set; }
    public string RegPhone { get; set; }

    private string _regRole;
    public string RegRole
    {
        get => _regRole;
        set
        {
            _regRole = value;
            Notify(nameof(RegRole));
        }
    }

    public string RegPassword { get; set; }
    public string RegError { get; set; }

    public ICommand LoginCommand => new AsyncRelayCommand(async _ => await Login());
    public ICommand RegisterCommand => new AsyncRelayCommand(Register);

    // ---------- LOGIN ----------
    private async Task Login()//void
    
    {
        if (string.IsNullOrWhiteSpace(LoginEmail) || string.IsNullOrWhiteSpace(LoginPassword))
        {
            MessageBox.Show("Введите email и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        //using var db = new Context();

        //var user = db.Users.FirstOrDefault(x => x.Email == LoginEmail);
        var user = await unitOfWork.Users.GetByEmailAsync(LoginEmail);

        if (user == null)
        {
            MessageBox.Show("Пользователь с таким email не найден.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (user.IsBlocked)
        {
            MessageBox.Show("Ваш аккаунт заблокирован.\nОбратитесь в поддержку.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!BCrypt.Net.BCrypt.Verify(LoginPassword, user.PasswordHash))
        {
            MessageBox.Show("Неверный пароль.\nПроверьте точность ввода.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        ApplicationContext.CurrentUser = user;
        //if (user.Role == 1)
        //    ApplicationContext.CurrentTutorId = user.Id;
        //else ApplicationContext.CurrentTutorId = null;

            MessageBox.Show($"Добро пожаловать, {user.FullName}!",
                "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);

        // Открытие главного окна
        var main = new MainWindow();
        Application.Current.MainWindow = main;
        main.Show();

        // Закрыть окно логина
        foreach (Window w in Application.Current.Windows)
            if (w is LoginRegisterWindow) { w.Close(); break; }
    }


    // ---------- REGISTER ----------


    private async Task Register(object obj)
    {
        // Email
        if (string.IsNullOrWhiteSpace(RegEmail) ||
            !Regex.IsMatch(RegEmail, 
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]\.[a-zA-Z]{2,6}$"))
        {
            MessageBox.Show("Введите корректный email.\nПример: example@gmail.com",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Full name
        if (string.IsNullOrWhiteSpace(RegFullName) || RegFullName.Length < 5)
        {
            MessageBox.Show("Введите корректное ФИО.\nМинимум 5 символов.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Phone
        if (string.IsNullOrWhiteSpace(RegPhone) ||
            !Regex.IsMatch(RegPhone, @"^\+375(29|33|44)\d{7}$"))
        {
            MessageBox.Show("Введите корректный номер телефона.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Role
        if (RegRole != "Клиент" && RegRole != "Репетитор")
        {
            MessageBox.Show("Выберите тип пользователя (Клиент или Репетитор).",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Password
        if (string.IsNullOrWhiteSpace(RegPassword) || RegPassword.Length < 6)
        {
            MessageBox.Show("Пароль должен быть длиной минимум 6 символов.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Regex.IsMatch(RegPassword, @"^(?=.*[A-Za-z])(?=.*\d).+$"))
        {
            MessageBox.Show("Пароль должен содержать хотя бы:\n• одну букву\n• одну цифру",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        //using var db = new Context();
        //if (db.Users.Any(x => x.Email == RegEmail))
       
        //bool 

        if (await unitOfWork.Users.GetByEmailAsync(RegEmail) != null)
        {
            MessageBox.Show("Этот email уже зарегистрирован.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int role = RegRole == "Репетитор" ? 1 : 0;

        var newUser = new User
        {
            FullName = RegFullName,
            Email = RegEmail,
            Phone = RegPhone,
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(RegPassword),
            IsBlocked = false
        };

        await unitOfWork.Users.AddAsync(newUser);
        await unitOfWork.SaveAsync();

        MessageBox.Show("Регистрация успешно выполнена!\nТеперь вы можете войти.",
            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }


    private void Notify(string prop)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
