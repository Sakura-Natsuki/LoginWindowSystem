using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LoginWindowSystem.Helpers
{
    public class RelayCommand : ICommand
    {
        //Action<object> 接受一个object参数，无返回值的方法
        private readonly Action<object> _execute;

        //Predicate<object> 接受一个object参数、返回bool的方法
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute,Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentException(nameof(execute));

            _canExecute = canExecute;
        }

        //ICommand的三个成员:Execute,CanExecute,CanExecuteChanged

        //WPF在按钮点击时调用此方法
        public void Execute(object parameter) => _execute(parameter);

        //WPF调用此方法判断按钮是否可用
        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        //"关键技巧"：将CanExecuteChanged事件转发给WPF全局的RequerySuggest
        //WPF在焦点变化、键盘输入等时机自动重新查询所有命令的CanExecute
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
