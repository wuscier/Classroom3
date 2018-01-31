using System.Windows.Input;
using Prism.Mvvm;

namespace Classroom.Model
{
    public class RadioPictureButtonItem:BindableBase
    {
        public string Image { get; set; }
        public string Name { get; set; }

        private bool _isChecked;

        public bool Checked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }

        public ICommand CheckCommand { get; set; }
        public object Type { get; set; }
        public string GroupName { get; set; }
    }
}
