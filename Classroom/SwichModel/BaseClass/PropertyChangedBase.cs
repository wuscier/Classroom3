using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Classroom.SwichModel.BaseClass
{
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public static class PropertyChangedBaseEx
    {
        public static void OnPropertyChanged<T, TProperty>(this T PropertyChangedBase, Expression<Func<T, TProperty>> propertyName) where T : PropertyChangedBase
        {
            var PropertyName = propertyName.Body as MemberExpression;
            if (PropertyName != null)
            {
                PropertyChangedBase.OnPropertyChanged(PropertyName.Member.Name);
            }
        }
    }
}
