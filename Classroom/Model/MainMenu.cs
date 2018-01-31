using System.Windows.Input;

namespace Classroom.Model
{
    public class MainMenu
    {
        public string MenuName { get; set; }
        public string ImageUrl { get; set; }
        public string FocusedImageUrl { get; set; }
        public ICommand GotoCommand { get; set; }
    }
}
