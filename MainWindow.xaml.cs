using CursorTail.Core;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using CL = CursorTail.Core.CursorLocation;

namespace CursorTail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Rope rope;
        public PainterVisionHost painter;
        public FrameController frameController;
        private DpiScale _dpiScale;
        private System.Drawing.Rectangle[] _screens;
        public MainWindowViewModel ViewModel;
        public StateMachine stateMachine;
        public GIFLoder gifLoder;
        public MainWindow()
        {
            InitializeComponent();
            _dpiScale = VisualTreeHelper.GetDpi(this);
            _screens = Screen.AllScreens.Select(s => s.WorkingArea).ToArray();
            SourceInitialized += OnSourceInitialized;
            this.SnapsToDevicePixels = true;

            //实例化
            stateMachine = new();
            rope = new Rope(stateMachine);
            //绑定鼠标跨屏事件
            CL.RaiseDeskTopChange += (System.Drawing.Rectangle bound) =>
            {
                this.Top = bound.Top;
                this.Left = bound.Left;
                this.Width = bound.Width / _dpiScale.DpiScaleX;
                this.Height = bound.Height / _dpiScale.DpiScaleY;
                rope.CollideBox = new(0,0, (float)Width, (float)Height);
            };
            this.WindowState = WindowState.Normal;
            gifLoder = new(stateMachine, 4);
            painter = new PainterVisionHost(rope, Color.FromRgb(255, 255, 0), Color.FromRgb(0, 0, 0), new Point(0, 0), 0, gifLoder);
            MainCanvas.Children.Add(painter);
            frameController = new FrameController(UpdatePerFrame, 60);
            ViewModel = new MainWindowViewModel(rope, painter, frameController, gifLoder);

            //绑定事件
            //CompositionTarget.Rendering += (s,e)=> frameController.UpdateFrame();
            LoadTaskIcon();
            LoadReTopFC();
            DispatcherTimer timer = new DispatcherTimer(TimeSpan.FromMicroseconds(6), DispatcherPriority.Render, (s, e) => frameController.UpdateFrame(), Dispatcher);
        }

        private void UpdatePerFrame()
        {
            CL.FrushCursorPos(_dpiScale);
            rope.Update(new(CL.RelatviCursorPos.X + ViewModel.CursorOffset_X, CL.RelatviCursorPos.Y + ViewModel.CursorOffset_Y));
            painter.Update();
        }

        /// <summary>
        /// 源初始化完毕函数，在构造函数完成后运行，窗口已有句柄，此时定义鼠标穿透和工具窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            const int WS_EX_TRANSPARENT = 0x00000020;
            const int WS_EX_TOOLWINDOW = 0x00000080;
            const WINDOW_LONG_PTR_INDEX GWL_EXSTYLE = (WINDOW_LONG_PTR_INDEX)(-20);
            var hwnd = new WindowInteropHelper(this).Handle;
            int currentExStyle = PInvoke.GetWindowLong(new HWND(hwnd), GWL_EXSTYLE);
            PInvoke.SetWindowLong(new HWND(hwnd), GWL_EXSTYLE, currentExStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        }

        SettingWindow? _settingWindow;
        private void LoadTaskIcon()
        {
            NotifyIcon notifyIcon = new NotifyIcon()
            {
                Text = "CursorTail",
                Icon = new System.Drawing.Icon(System.IO.Path.Combine(AppContext.BaseDirectory, "Core/icon.ico")),
            };
            EventHandler openSetting = (s, e) =>
            {
                if (_settingWindow == null)
                {
                    _settingWindow = new SettingWindow(ViewModel, painter);
                    _settingWindow.Closed += (s, e) => _settingWindow = null;
                    _settingWindow.Show();
                }
                else
                {
                    _settingWindow.Topmost = true;
                    _settingWindow.Topmost = false;
                }
            };
            notifyIcon.DoubleClick += openSetting;
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem setting = new("设置");
            setting.Click += openSetting;
            ToolStripMenuItem exit = new("退出");
            exit.Click += (s, e) => this.Close();
            contextMenu.Items.Add(setting);
            contextMenu.Items.Add(exit);
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Visible = true;
            this.Closing += (s, e) =>
            {
                notifyIcon.Dispose();
                if (_settingWindow != null)
                {
                    _settingWindow.Close();
                }
            };
        }

        private void LoadReTopFC()
        {
            FrameController fc = new FrameController(() =>
            {
                this.Topmost = false;
                this.Topmost = true;
            })
            {
                TargetFrameTime = 10 * 1000,
            };
            frameController.UpdatePerFrame += fc.UpdateFrame;
        }
    }
}