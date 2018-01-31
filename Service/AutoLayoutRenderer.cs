using MeetingSdk.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MeetingSdk.NetAgent.Models;
using System.Windows;
using Caliburn.Micro;

namespace Service
{
    public class AutoLayoutRenderer : ILayoutRenderer
    {
        //public bool CanRender(IVideoBoxManager videoBoxManager, out string cannotRenderMessage)
        //{
        //    cannotRenderMessage = string.Empty;
        //    return true;
        //}

        //public void CalculateViewBoxPositions(IVideoBoxManager videoBoxManager, double width, double height, string key)
        //{
        //    ModeDisplayerFactory.Factory.Create().CalculateViewBoxPositions(videoBoxManager, width, height, key);
        //}

        //public void Render(IVideoBoxManager videoBoxManager)
        //{

        //}

        //private bool TryGetContainer(IVideoBoxManager videoBoxManager, out Canvas canvas)
        //{
        //    canvas = null;

        //    if (videoBoxManager == null)
        //    {
        //        return false;
        //    }

        //    if (!videoBoxManager.Any())
        //    {
        //        return false;
        //    }

        //    canvas = videoBoxManager.First().Host?.Parent as Canvas;

        //    return canvas != null;
        //}

        public void Render(IEnumerable<IVideoBox> videoBoxs, Size canvasSize)
        {
            IMeetingWindowManager meetingWindowManager = IoC.Get<IMeetingWindowManager>();
            meetingWindowManager.ModeDisplayerStore.Create().Display(videoBoxs, canvasSize);
        }
    }
}
