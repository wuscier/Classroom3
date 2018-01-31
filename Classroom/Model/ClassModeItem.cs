using Prism.Mvvm;

namespace Classroom.Model
{
    public class ClassModeItem : BindableBase
    {
        private string _classModeName;

        public string ClassModeName
        {
            get { return _classModeName; }
            set { SetProperty(ref _classModeName, value); }
        }

        private string _classModeImage;
        public string ClassModeImage
        {
            get { return _classModeImage; }
            set { SetProperty(ref _classModeImage, value); }
        }
    }
}
