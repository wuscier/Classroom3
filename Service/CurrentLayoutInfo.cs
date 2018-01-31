using Common.Model.ViewLayout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class CurrentLayoutInfo
    {
        private ClassMode _classMode;
        public ClassMode ClassMode
        {
            get
            {
                return _classMode;
            }
            set
            {
                _classMode = value;
                ClassModeChangedEvent?.Invoke(value);
                //LayoutChangedEvent?.Invoke();
            }
        }

        private PictureMode _pictureMode;
        public PictureMode PictureMode
        {
            get { return _pictureMode; }
            set
            {
                _pictureMode = value;
                PictureModeChangedEvent?.Invoke(value);
                //LayoutChangedEvent?.Invoke();
            }
        }

        public delegate void ClassModeChanged(ClassMode classMode);
        public delegate void PictureModeChanged(PictureMode pictureMode);

        public event ClassModeChanged ClassModeChangedEvent;
        public event PictureModeChanged PictureModeChangedEvent;
        //public event Action LayoutChangedEvent;

        private CurrentLayoutInfo()
        {
            PictureMode = PictureMode.AverageMode;
            ClassMode = ClassMode.InteractionMode;
        }

        public static readonly CurrentLayoutInfo Instance = new CurrentLayoutInfo();
    }
}
