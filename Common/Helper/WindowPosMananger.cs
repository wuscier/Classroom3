using System;
using System.Collections.Generic;

namespace Common.Helper
{
    public class WindowPosMananger
    {
        private List<IntptrData> ptrs = new List<IntptrData>();

        public static bool ExitFlag = false;

        public static bool ReciveMeeting = false;

        private IntPtr _keyBoardPtr = IntPtr.Zero;
        private bool _IsGetFocusFrequent = true;
        private static WindowPosMananger _instance = null;
        private static readonly object LocalObject = new object();

        public static WindowPosMananger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WindowPosMananger();
                    // _instance._IsGetFocusFrequent = MeetingControlSetting.Instance.IsGetFocusFrequent;
                }
                return _instance;
            }
        }


        public void SetIntPrt(IntPtr Ptr, int topMost)
        {
            lock (LocalObject)
            {
                IntptrData data = new IntptrData();
                data.PrtHandle = Ptr;
                data.Top = topMost;
                ptrs.Add(data);
            }
        }




        public void SetKeyBoardIntptr(IntPtr keyBoardPtr)
        {
            lock (LocalObject)
            {
                _keyBoardPtr = keyBoardPtr;
            }
        }


        //public int SW_SHOWNOACTIVATE = 4;
        //public int SWP_NOSIZE = 0x0001;
        //public int SWP_NOMOVE = 0x0002;
        //public int SWP_SHOWWINDOW = 0x0040;
        //public int HWND_TOP = 0;


        public class IntptrData
        {
            public IntPtr PrtHandle { get; set; }

            public int Top { get; set; }
        }
    }
}
