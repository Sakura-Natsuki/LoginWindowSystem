using LoginWindowSystem.Helpers;
using LoginWindowSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace LoginWindowSystem.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly DatabaseService _db = new DatabaseService();

        private string _username;

        public string UserName
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword;

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        private string _nickname;

        public string NickName
        {
            get => _nickname;
            set => SetProperty(ref _nickname, value);
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _isRegistering;

        public bool IsRegistering
        {
            get => _isRegistering;
            set => SetProperty(ref _isRegistering, value);
        }

        public ICommand RegisterCommand { get; }

        public ICommand BackToLoginCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(async _ => await ExecuteRegister(),_ => CanRegister());
            BackToLoginCommand = new RelayCommand(_ => ExecuteBackToLogin());
        }

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(UserName)
                && !string.IsNullOrWhiteSpace(Password)
                && !string.IsNullOrWhiteSpace(ConfirmPassword)
                && !string.IsNullOrWhiteSpace(NickName)
                && !IsRegistering;
        }

        private async Task ExecuteRegister()
        {
            ErrorMessage = string.Empty;

            if(Password != ConfirmPassword)
            {
                ErrorMessage = "Password do not match";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters";
                return;
            }

            IsRegistering = true;

            try
            {
                bool success = await Task.Run(() => _db.RegisterUser(UserName, Password, NickName));

                if (success)
                {
                    LogService.Instacne.Info($"Registration successful:{UserName} ({NickName})");

                    var mainWin = new Views.MainWindow(NickName);

                    mainWin.Show();

                    CloseWindow<Views.LoginWindow>();
                    CloseWindow<Views.RegisterWindow>();
                }
                else
                {
                    ErrorMessage = "Registration failed: The username is already taken.";
                    LogService.Instacne.Warn($"Registration failed (The username is already taken) :{UserName} .");
                }
                
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
                LogService.Instacne.Error($"Registration failed: {ex.Message}");
            }
            finally
            {
                IsRegistering = false;
            }
        }

        private void ExecuteBackToLogin()
        {
            new Views.LoginWindow().Show();

            CloseWindow<Views.RegisterWindow>();
        }

        private void CloseWindow<T>() where T : Window
        {
            foreach (Window window in Application.Current.Windows)
            {
                if(window is T)
                {
                    window.Close();

                    break;
                }
            }
        }
    }
}
