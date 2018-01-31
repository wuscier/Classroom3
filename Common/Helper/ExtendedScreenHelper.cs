using Common.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Common.Helper
{
    public class ExtendedScreenHelper
    {
        public static readonly ExtendedScreenHelper Instance = new ExtendedScreenHelper();

        public int ExtendScreenPosition { get; set; }

        public int ExtendScreenWidth { get; set; }

        public int ExtendScreenHeight { get; set; }

        public List<ScreenModel> Screens { get; set; }
        public bool IsDoubleScreenOn { get; set; }

        public bool IsExtendedMode()
        {
            try
            {
                Screen[] screens = Screen.AllScreens;
                int screenCount = screens.Length;
                Log.Logger.Information($"扩展屏数量：{screenCount}");
                if (screenCount <= 1)
                {
                    return false;
                }
                else
                {
                    Screens = new List<ScreenModel>();
                    Screens.Add(new ScreenModel()
                    {
                        X = screens[0].Bounds.X,
                        Y = screens[0].Bounds.Y,
                        Width = screens[0].Bounds.Width,
                        Height = screens[0].Bounds.Height,
                        Type = 1
                    });
                    Screens.Add(new ScreenModel()
                    {
                        X = screens[1].Bounds.X,
                        Y = screens[1].Bounds.Y,
                        Width = screens[1].Bounds.Width,
                        Height = screens[1].Bounds.Height,
                        Type = 2
                    });
                    Log.Logger.Information($"扩展屏信息1：{screens[0].Bounds.X},{screens[0].Bounds.Width},{screens[1].Bounds.X},{screens[1].Bounds.Width}");
                    var mainScreen = Screens.FirstOrDefault(o => o.X == 0);
                    var extendedScreen = Screens.FirstOrDefault(o => o.X != 0);
                    if (mainScreen == null || extendedScreen == null) return false;
                    mainScreen.Type = 1;
                    extendedScreen.Type = 2;
                    var result = true;
                    ExtendScreenPosition = extendedScreen.X >= 0 ? mainScreen.Width : extendedScreen.X;
                    ExtendScreenWidth = extendedScreen.Width;
                    ExtendScreenHeight = extendedScreen.Height;

                    //ExtendScreenWidth = 1680;
                    //ExtendScreenHeight = 945;

                    Log.Logger.Information($"扩展屏信息宽度与高度：{ExtendScreenWidth}---{ExtendScreenHeight}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}
