using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace CursorTail.Core
{
    public delegate void RaiseHotKeyEventHandler(int id);
    public enum HotKeyVk : uint
    {
        X = 0x58,
        Z = 0x5A,
    }
    public class HotKeyManager : IDisposable
    {
        private List<Action> _callBacks;
        private HotKeyRecWnd _recWnd;

        const HOT_KEY_MODIFIERS _modify = HOT_KEY_MODIFIERS.MOD_ALT | HOT_KEY_MODIFIERS.MOD_CONTROL| HOT_KEY_MODIFIERS.MOD_SHIFT| HOT_KEY_MODIFIERS.MOD_NOREPEAT;

        public void Dispose()
        {
            for (int i = 0; i < _callBacks.Count; i++)
            {
                RemoveHotKey(i);
            }
            _callBacks.Clear();
            _recWnd.DestroyHandle();
        }
        public bool SetHotKey(HotKeyVk vk, Action callBack)
        {
            int id = _callBacks.Count;
            _callBacks.Add(callBack);
            return PInvoke.RegisterHotKey(new(_recWnd.Handle), id, _modify, (uint)vk);
        }
        public void RemoveHotKey(int id)
        {
            if(id < _callBacks.Count)
            {
                PInvoke.UnregisterHotKey(new(_recWnd.Handle), id);
                _callBacks.RemoveAt(id);
            }
        }
        public HotKeyManager()
        {
            _callBacks = new List<Action>();
            _recWnd = new HotKeyRecWnd(RaiseHotKeyEvent);
        }
        public void RaiseHotKeyEvent(int id)
        {
            if(id<_callBacks.Count)
            {
                _callBacks[id].Invoke();
            }
        }
    }
    public class HotKeyRecWnd : NativeWindow
    {
        const int WM_HOTKEY = 0x0312;
        private RaiseHotKeyEventHandler RaiseHotKeyEvent;
        public HotKeyRecWnd(RaiseHotKeyEventHandler raiseHotKeyEvent)
        {
            RaiseHotKeyEvent = raiseHotKeyEvent;
            CreateHandle(new CreateParams()
            {
                Style = 0,
                ExStyle = 0,
            });
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_HOTKEY)
            {
                RaiseHotKeyEvent.Invoke((int)m.WParam);
            }
        }
    }
}
