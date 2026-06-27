using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Windows;
using Windows.Win32;
using Point = System.Drawing.Point;

namespace CursorTail.Core
{
    public class CursorLocation
    {
        public static Point CursorPos = new(0, 0);
        public static Vector2 RelatviCursorPos = new(0, 0);
        public static Screen? CurrentScreen;
        public delegate void DeskTopChangeEventHandler(Rectangle bounds);
        public static DeskTopChangeEventHandler? RaiseDeskTopChange;
        public static void FrushCursorPos()
        {
            PInvoke.GetCursorPos(out CursorPos);
            if (CurrentScreen==null || CurrentScreen.DeviceName != Screen.FromPoint(CursorPos).DeviceName)
            {
                CurrentScreen = Screen.FromPoint(CursorPos);
                RaiseDeskTopChange?.Invoke(CurrentScreen.Bounds);
            }

        }
        public static void FrushCursorPos(DpiScale dpi)
        {
            PInvoke.GetCursorPos(out CursorPos);
            if (CurrentScreen == null || CurrentScreen.DeviceName != Screen.FromPoint(CursorPos).DeviceName)
            {
                CurrentScreen = Screen.FromPoint(CursorPos);
                RaiseDeskTopChange?.Invoke(CurrentScreen.Bounds);
            }
            RelatviCursorPos.X = (CursorPos.X - CurrentScreen.Bounds.Left) / (float)dpi.DpiScaleX;
            RelatviCursorPos.Y = (CursorPos.Y - CurrentScreen.Bounds.Top) / (float)dpi.DpiScaleY;
        }
    }
}
