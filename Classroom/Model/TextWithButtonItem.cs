using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Prism.Mvvm;

namespace Classroom.Model
{
    public class TextWithButtonItem : BindableBase
    {

        public string Id { get; set; }
        public IntPtr Hwnd { get; set; }


        public ICommand ButtonCommand { get; set; }
        public string Text { get; set; }

        private string _content;
        public string Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }

        private Brush _buttonForeground;
        public Brush ButtonForeground
        {
            get { return _buttonForeground; }
            set { SetProperty(ref _buttonForeground, value); }
        }

        private Brush _buttonBackground;
        public Brush ButtonBackground
        {
            get { return _buttonBackground; }
            set { SetProperty(ref _buttonBackground, value); }
        }

        private Visibility _buttonVisibility;
        public Visibility ButtonVisibility
        {
            get { return _buttonVisibility; }
            set { SetProperty(ref _buttonVisibility, value); }
        }
    }
}
