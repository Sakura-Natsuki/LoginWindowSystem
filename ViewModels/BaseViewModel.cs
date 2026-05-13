using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace LoginWindowSystem.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        //1.事件声明：当属性改变时：触发这个事件
        public event PropertyChangedEventHandler PropertyChanged;

        //2.触发事件的方法：通知UI名为propertyName的属性已经被改变 
        // [CallerMemberName] :编译器自动把调用此方法的属性名填进去
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //3.带值比较的便捷方法
        //先比较新旧值，如果相同就不触发。
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }
    }
}
