using LoginWindowSystem.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LoginWindowSystem.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        string _nickNmae;

        string _currentTime;

        string _welcomeMessage;

        public string Nickname
        {
            get => _nickNmae;
            set => SetProperty(ref _nickNmae, value);
        }

        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public ICommand RefreshCommand { get; }

        public ICommand DisconnectCommand { get; }

        public MainViewModel(string nickname)
        {
            Nickname = nickname ?? "Unknown";

            WelcomeMessage = $"欢迎回来, {Nickname}";

            UpdateTime();

            RefreshCommand = new RelayCommand(_ => UpdateTime());

            DisconnectCommand = new RelayCommand(_ => ExecuteDisconnect());
        }

        private void UpdateTime()
        {
            CurrentTime = DateTime.Now.ToString("yyyy HH:mm:ss");
        }

        private void ExecuteDisconnect()
        {
            new Views.LoginWindow().Show(); 

            foreach (Window w in Application.Current.Windows)
            {
                if (w is Views.MainWindow)
                {
                    w.Close();
                    break;
                }
            }
        }
    }
}