using LoginWindowSystem.Helpers;
using LoginWindowSystem.Models;
using LoginWindowSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LoginWindowSystem.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly DatabaseService _db = new DatabaseService();

        private string _username;

        public string Username
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

        private string _errorMessage;

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _isLogging;

        public bool IsLogging
        {
            get => _isLogging;
            set => SetProperty(ref _isLogging, value);
        }

        public ICommand LoginCommand { get; }

        public ICommand OpenRegisterCommand { get; }

        public LoginViewModel()
        {
            OpenRegisterCommand = new RelayCommand(_ => ExecuteOpenRegister());

            LoginCommand = new RelayCommand(async _ => await ExecuteLogin(), _ => CanLogin());
        }

        private bool CanLogin()
        {
            //1. 用户名不为空或纯空格
            //2. 密码不为空或纯空格
            //3. 当前不在登录过程中（防止重复提交）
            return !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password)
                && !IsLogging;
        }

        private async Task ExecuteLogin()
        {
            ErrorMessage = string.Empty;

            IsLogging = true;

            try
            {
                var user = await Task.Run(() => _db.ValidateLogin(Username,Password));

                if (user != null)
                {
                    var mainWin = new Views.MainWindow(user.Nickname);

                    mainWin.Show();

                    foreach (Window w in Application.Current.Windows)
                    {
                        if (w is Views.LoginWindow)
                        {
                            w.Close();
                            break;
                        }
                    }
                }
                else
                {
                    ErrorMessage = "用户名或密码错误";
                }
            }
            catch(Exception ex)
            {
                ErrorMessage = $"登录失败: {ex.Message}";
            }
            finally
            {
                IsLogging = false;
            }
        }

        private void ExecuteOpenRegister()
        {
            Services.LogService.Instacne.Info("User clicks the registration entry");

            new Views.RegisterWindow().Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (window is Views.LoginWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
