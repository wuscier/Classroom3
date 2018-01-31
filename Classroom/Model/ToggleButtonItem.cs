using System.Windows.Input;
using Prism.Mvvm;

namespace Classroom.Model
{
    public class ToggleButtonItem : BindableBase
    {
        public ICommand ToggleCommand { get; set; }
        public string Name { get; set; }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }

        private string _tips;

        public string Tips
        {
            get { return _tips; }
            set { SetProperty(ref _tips, value); }
        }
    }
}
