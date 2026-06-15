using LoginWindowSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LoginWindowSystem.Views
{
    /// <summary>
    /// RegisterWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();

            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void PwdBox_PasswordChanged(object sender,RoutedEventArgs e)
        {
            if (DataContext is ViewModels.RegisterViewModel vm)
            {
                vm.Password = PwdBox.Password;
            }
        }

        private void ConfirmPwdBox_PasswordChanged(object sender,RoutedEventArgs e)
        {
            if (DataContext is ViewModels.RegisterViewModel vm)
            {
                vm.ConfirmPassword = ConfirmPwdBox.Password;
            }
        }

        private void BackToLLogin_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ViewModels.RegisterViewModel vm)
            {
                vm.BackToLoginCommand.Execute(null);
            }
        }
    }
}
